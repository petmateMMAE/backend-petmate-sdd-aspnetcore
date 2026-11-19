using PetMate.Domain.Carnet;
using PetMate.Domain.Chat;
using PetMate.Domain.Common;
using PetMate.Domain.Followups;

namespace PetMate.UnitTests.Domain;

/// <summary>pm-131, pm-132, pm-138 a pm-140, pm-143, pm-144, pm-147: consultas, receta, seguimiento y conversación.</summary>
public sealed class VeterinaryDomainTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = new(2026, 11, 10);
    private static readonly RecordProvenance Vet = RecordProvenance.ByBusiness(Guid.NewGuid(), "Vet Sol");

    private static MedicationDetails Med(string name = "Omeprazol") => new(name, "10 mg", "cada 12 h", "7 días", null);

    [Fact]
    public void A_consultation_keeps_the_business_origin_the_optional_weight_and_the_prescription()
    {
        var consultation = Consultation.Create(Guid.NewGuid(), Guid.NewGuid(), Vet, new ConsultationDetails("  Vómito ", " Gastritis ", 8.4m, [Med(), Med("Probiótico")]), Today, Now);

        Assert.Equal(("Vómito", "Gastritis", 8.4m), (consultation.Reason, consultation.Diagnosis, consultation.WeightKg));
        Assert.Equal(2, consultation.Medications.Count);
        Assert.False(consultation.CanOwnerModify);
        Assert.Null(Consultation.Create(Guid.NewGuid(), Guid.NewGuid(), Vet, new ConsultationDetails("X", "Y", null, null), Today, Now).WeightKg);
    }

    [Fact]
    public void A_consultation_needs_reason_and_diagnosis_a_valid_weight_and_cannot_come_from_the_owner()
    {
        Assert.Equal(["diagnosis", "reason", "weightKg"], new ConsultationDetails(" ", null, 0m, null).InvalidFields().Order(StringComparer.Ordinal));
        Assert.Throws<DomainException>(() => Consultation.Create(Guid.NewGuid(), Guid.NewGuid(), RecordProvenance.ByOwner, new ConsultationDetails("X", "Y", null, null), Today, Now));
    }

    [Fact]
    public void The_prescription_has_a_maximum_of_ten_medications_and_each_one_needs_its_data()
    {
        var consultation = Consultation.Create(Guid.NewGuid(), Guid.NewGuid(), Vet, new ConsultationDetails("X", "Y", null, null), Today, Now);

        for (var i = 0; i < Consultation.MaxMedications; i++)
        {
            consultation.AddMedication(Med());
        }

        Assert.Throws<DomainException>(() => consultation.AddMedication(Med()));
        Assert.Contains("medications", new ConsultationDetails("X", "Y", null, Enumerable.Range(0, 11).Select(_ => Med()).ToArray()).InvalidFields());
        Assert.Equal(["dose", "duration", "frequency", "name"], new MedicationDetails(null, null, null, null, null).InvalidFields().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void A_followup_opens_treating_with_an_optional_review_of_3_7_or_15_days()
    {
        var followup = Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "Firulais", Guid.NewGuid(), Guid.NewGuid(), 7, Today, Now);

        Assert.Equal((FollowupStatus.Treating, Today.AddDays(7)), (followup.Status, followup.NextReviewOn));
        Assert.Null(Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "X", Guid.NewGuid(), Guid.NewGuid(), null, Today, Now).NextReviewOn);
        Assert.Throws<DomainException>(() => Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "X", Guid.NewGuid(), Guid.NewGuid(), 5, Today, Now));
    }

    [Fact]
    public void Discharging_removes_the_review_and_an_open_followup_keeps_it_unless_a_new_one_is_given()
    {
        var followup = Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "Firulais", Guid.NewGuid(), Guid.NewGuid(), 7, Today, Now);

        followup.Update(FollowupStatus.Improving, null, Today, Now);
        Assert.Equal(Today.AddDays(7), followup.NextReviewOn);
        followup.Update(FollowupStatus.Improving, 3, Today, Now);
        Assert.Equal(Today.AddDays(3), followup.NextReviewOn);
        followup.Update(FollowupStatus.Discharged, 15, Today, Now);
        Assert.Null(followup.NextReviewOn);
        followup.Reopen(Now.AddHours(1));
        Assert.Equal(FollowupStatus.Treating, followup.Status);
    }

    [Fact]
    public void A_review_is_overdue_only_when_it_already_passed_and_the_followup_is_not_discharged()
    {
        var followup = Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "Firulais", Guid.NewGuid(), Guid.NewGuid(), 3, Today, Now);

        Assert.False(followup.IsOverdue(Today.AddDays(3)));
        Assert.True(followup.IsOverdue(Today.AddDays(4)));
        followup.Update(FollowupStatus.Discharged, null, Today, Now);
        Assert.False(followup.IsOverdue(Today.AddDays(30)));
    }

    [Fact]
    public void Notes_keep_their_order_and_reject_empty_or_oversized_text()
    {
        var followup = Followup.Open(Guid.NewGuid(), Guid.NewGuid(), "Firulais", Guid.NewGuid(), Guid.NewGuid(), null, Today, Now);

        followup.AddNote("  Primera ", Now);
        followup.AddNote("Segunda", Now.AddMinutes(1));

        Assert.Equal(["Primera", "Segunda"], followup.Notes.Select(n => n.Text));
        Assert.Throws<DomainException>(() => followup.AddNote("  ", Now));
        Assert.Throws<DomainException>(() => followup.AddNote(new string('a', Followup.MaxNoteLength + 1), Now));
    }

    [Fact]
    public void A_conversation_has_exactly_two_distinct_participants()
    {
        var owner = Guid.NewGuid();
        var business = Guid.NewGuid();
        var conversation = Conversation.Open(Guid.NewGuid(), Guid.NewGuid(), owner, Guid.NewGuid(), business, Now);

        Assert.True(conversation.HasParticipant(owner));
        Assert.True(conversation.HasParticipant(business));
        Assert.False(conversation.HasParticipant(Guid.NewGuid()));
        Assert.True(conversation.IsBusinessSide(business));
        Assert.False(conversation.IsBusinessSide(owner));
        Assert.Throws<DomainException>(() => Conversation.Open(Guid.NewGuid(), Guid.NewGuid(), owner, Guid.NewGuid(), owner, Now));
    }

    [Theory]
    [InlineData("Hola", true)]
    [InlineData("  ", false)]
    [InlineData("", false)]
    [InlineData("línea 1\nlínea 2", true)]
    [InlineData("mal\u0007texto", false)]
    public void A_message_needs_text_without_control_characters(string text, bool valid)
        => Assert.Equal(valid, Message.IsValidText(text));

    [Fact]
    public void A_message_has_a_sender_and_a_system_notice_has_none_and_the_length_is_bounded()
    {
        var conversation = Guid.NewGuid();
        var sender = Guid.NewGuid();

        Assert.Equal((MessageKind.Text, sender), (Message.FromParticipant(conversation, 1, sender, " Hola ", Now).Kind, Message.FromParticipant(conversation, 1, sender, " Hola ", Now).SenderAccountId));
        Assert.Null(Message.FromSystem(conversation, 2, "Aviso", Now).SenderAccountId);
        Assert.False(Message.IsValidText(new string('a', Message.MaxTextLength + 1)));
        Assert.Throws<DomainException>(() => Message.FromParticipant(conversation, 3, sender, " ", Now));
    }
}
