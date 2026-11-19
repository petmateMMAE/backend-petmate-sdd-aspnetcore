using PetMate.Domain.Access;
using PetMate.Domain.Carnet;
using PetMate.Domain.Chat;
using PetMate.Domain.Followups;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// pm-130 a pm-149 contra la base Atlas de pruebas: consultas con receta, seguimiento único por consulta, conversación única por
/// seguimiento, secuencia atómica de mensajes, estado de lectura por lado y registro de escaneos. Skipped si no hay credenciales.
/// </summary>
public sealed class VeterinaryPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task A_consultation_round_trips_with_its_prescription_and_only_its_author_can_change_it()
    {
        await using var context = await MongoTestContext.Create();
        var consultations = new MongoConsultationRepository(context.Database, context.Names);
        var owner = Guid.NewGuid();
        var business = Guid.NewGuid();
        var consultation = Consultation.Create(Guid.NewGuid(), owner, RecordProvenance.ByBusiness(business, "Vet Sol"), new ConsultationDetails("Vómito", "Gastritis", 8.4m, [new MedicationDetails("Omeprazol", "10 mg", "12 h", "7 días", "En ayunas")]), Today, Now);
        await consultations.Add(consultation, CancellationToken.None);

        var stored = await consultations.FindOwned(consultation.Id, owner, CancellationToken.None);
        Assert.Equal(("Vómito", 8.4m), (stored!.Reason, stored.WeightKg));
        Assert.Equal("Omeprazol", Assert.Single(stored.Medications).Name);
        Assert.Null(await consultations.FindOwned(consultation.Id, Guid.NewGuid(), CancellationToken.None));
        Assert.Null(await consultations.FindByIdForBusiness(consultation.Id, Guid.NewGuid(), CancellationToken.None));

        var author = await consultations.FindByIdForBusiness(consultation.Id, business, CancellationToken.None);
        author!.AddMedication(new MedicationDetails("Probiótico", "1", "1/día", "5 días", null));
        Assert.True(await consultations.ReplaceForBusiness(author, business, CancellationToken.None));
        Assert.False(await consultations.ReplaceForBusiness(author, Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(2, (await consultations.FindOwned(consultation.Id, owner, CancellationToken.None))!.Medications.Count);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Followups_are_unique_per_consultation_and_conversations_unique_per_followup()
    {
        await using var context = await MongoTestContext.Create();
        var followups = new MongoFollowupRepository(context.Database, context.Names);
        var conversations = new MongoConversationRepository(context.Database, context.Names);
        var consultationId = Guid.NewGuid();
        var owner = Guid.NewGuid();
        var business = Guid.NewGuid();
        var first = Followup.Open(consultationId, Guid.NewGuid(), "Firulais", owner, business, 7, Today, Now);

        Assert.True(await followups.TryAdd(first, CancellationToken.None));
        Assert.False(await followups.TryAdd(Followup.Open(consultationId, Guid.NewGuid(), "Otro", owner, business, null, Today, Now), CancellationToken.None));
        Assert.Equal(first.Id, (await followups.FindByConsultation(consultationId, owner, CancellationToken.None))!.Id);
        Assert.Null(await followups.FindByConsultation(consultationId, Guid.NewGuid(), CancellationToken.None));
        Assert.Null(await followups.FindForBusiness(first.Id, Guid.NewGuid(), CancellationToken.None));

        first.AddNote("Nota", Now);
        first.Update(FollowupStatus.Discharged, null, Today, Now);
        Assert.True(await followups.Update(first, CancellationToken.None));
        var reloaded = await followups.FindForBusiness(first.Id, business, CancellationToken.None);
        Assert.Equal((FollowupStatus.Discharged, 1), (reloaded!.Status, reloaded.Notes.Count));
        Assert.Equal(0, await followups.CountByBusiness(business, null, true, CancellationToken.None));
        Assert.Equal(1, await followups.CountByBusiness(business, null, false, CancellationToken.None));

        var conversation = Conversation.Open(first.Id, first.PetId, owner, business, Guid.NewGuid(), Now);
        Assert.True(await conversations.TryAdd(conversation, CancellationToken.None));
        Assert.False(await conversations.TryAdd(Conversation.Open(first.Id, first.PetId, owner, business, Guid.NewGuid(), Now), CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Message_sequences_are_consecutive_under_concurrency_and_read_state_is_per_side_and_never_goes_back()
    {
        await using var context = await MongoTestContext.Create();
        var conversations = new MongoConversationRepository(context.Database, context.Names);
        var messages = new MongoMessageRepository(context.Database, context.Names);
        var owner = Guid.NewGuid();
        var businessAccount = Guid.NewGuid();
        var conversation = Conversation.Open(Guid.NewGuid(), Guid.NewGuid(), owner, Guid.NewGuid(), businessAccount, Now);
        await conversations.TryAdd(conversation, CancellationToken.None);

        var seqs = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => conversations.NextSeq(conversation.Id, Now, CancellationToken.None)));
        Assert.Equal(Enumerable.Range(1, 20).Select(i => (long)i), seqs.Order());

        foreach (var seq in seqs.Order())
        {
            await messages.Add(Message.FromParticipant(conversation.Id, seq, seq % 2 == 0 ? owner : businessAccount, $"m{seq}", Now), CancellationToken.None);
        }

        await conversations.MarkRead(conversation.Id, businessSide: true, 10, CancellationToken.None);
        await conversations.MarkRead(conversation.Id, businessSide: true, 5, CancellationToken.None);
        var stored = await conversations.FindById(conversation.Id, CancellationToken.None);
        Assert.Equal((10L, 0L, 20L), (stored!.BusinessReadSeq, stored.OwnerReadSeq, stored.LastSeq));
        Assert.Equal(5, await messages.CountUnread(conversation.Id, stored.BusinessReadSeq, businessAccount, CancellationToken.None));
        Assert.Equal([11L, 12L], (await messages.List(conversation.Id, 10, 2, CancellationToken.None)).Select(m => m.Seq));
        Assert.Equal(20, (await messages.Last(conversation.Id, CancellationToken.None))!.Seq);
        Assert.Equal(20, await messages.DeleteByConversations([conversation.Id], CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Scan_logs_list_the_business_attended_pets_newest_first_with_the_since_filter()
    {
        await using var context = await MongoTestContext.Create();
        var logs = new MongoScanLogRepository(context.Database, context.Names);
        var business = Guid.NewGuid();
        await logs.Add(ScanLog.Record(Guid.NewGuid(), "Antiguo", null, business, Now.AddDays(-3)), CancellationToken.None);
        await logs.Add(ScanLog.Record(Guid.NewGuid(), "Reciente", "https://cdn.example.com/f.jpg", business, Now), CancellationToken.None);
        await logs.Add(ScanLog.Record(Guid.NewGuid(), "De otro", null, Guid.NewGuid(), Now), CancellationToken.None);

        var all = await logs.ListByBusiness(business, null, 0, 10, CancellationToken.None);
        var recent = await logs.ListByBusiness(business, Now.AddDays(-1), 0, 10, CancellationToken.None);

        Assert.Equal(["Reciente", "Antiguo"], all.Select(l => l.PetName));
        Assert.Equal("https://cdn.example.com/f.jpg", all[0].PhotoUrl);
        Assert.Equal(["Reciente"], recent.Select(l => l.PetName));
        Assert.Equal(2, await logs.CountByBusiness(business, null, CancellationToken.None));
    }
}
