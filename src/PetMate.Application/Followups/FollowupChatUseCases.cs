using PetMate.Application.Access;
using PetMate.Application.Businesses;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Chat;
using PetMate.Domain.Followups;

namespace PetMate.Application.Followups;

// ---------- Repositorios ----------

/// <summary>Seguimientos de consultas (pm-138 a pm-142). Uno por consulta; lo lleva la veterinaria autora.</summary>
public interface IFollowupRepository
{
    /// <summary>Guarda el seguimiento; <c>false</c> si la consulta ya tiene uno (índice único, pm-138#CA-02).</summary>
    Task<bool> TryAdd(Followup followup, CancellationToken cancellationToken);

    /// <summary>Seguimiento del negocio; ajeno o inexistente → <c>null</c>.</summary>
    Task<Followup?> FindForBusiness(Guid followupId, Guid businessId, CancellationToken cancellationToken);

    /// <summary>Seguimiento de una consulta del dueño; ajeno o inexistente → <c>null</c>.</summary>
    Task<Followup?> FindByConsultation(Guid consultationId, Guid ownerId, CancellationToken cancellationToken);

    Task<Followup?> FindById(Guid followupId, CancellationToken cancellationToken);

    Task<bool> Update(Followup followup, CancellationToken cancellationToken);

    /// <summary>Seguimientos del negocio, con filtro por día de revisión y por «en seguimiento» (no dados de alta).</summary>
    Task<IReadOnlyList<Followup>> ListByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, int skip, int take, CancellationToken cancellationToken);

    Task<long> CountByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, CancellationToken cancellationToken);

    Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken);
}

/// <summary>Conversaciones (pm-143). Los números de secuencia se asignan de forma atómica.</summary>
public interface IConversationRepository
{
    /// <summary>Guarda la conversación; <c>false</c> si el seguimiento ya tiene una (pm-143#CA-02).</summary>
    Task<bool> TryAdd(Conversation conversation, CancellationToken cancellationToken);

    Task<Conversation?> FindById(Guid conversationId, CancellationToken cancellationToken);

    Task<Conversation?> FindByFollowup(Guid followupId, CancellationToken cancellationToken);

    /// <summary>Conversaciones de la cuenta (como dueño o como veterinaria), la de actividad más reciente primero.</summary>
    Task<IReadOnlyList<Conversation>> ListForAccount(Guid accountId, int skip, int take, CancellationToken cancellationToken);

    Task<long> CountForAccount(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Reserva el siguiente número de secuencia y actualiza la actividad (operación atómica).</summary>
    Task<long> NextSeq(Guid conversationId, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Marca como leído hasta la secuencia dada para un lado; nunca retrocede.</summary>
    Task MarkRead(Guid conversationId, bool businessSide, long upToSeq, CancellationToken cancellationToken);

    Task SetClosed(Guid conversationId, bool closed, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Identificadores de las conversaciones de una mascota (para la cascada).</summary>
    Task<IReadOnlyList<Guid>> IdsByPet(Guid petId, CancellationToken cancellationToken);

    Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken);
}

public interface IMessageRepository
{
    Task Add(Message message, CancellationToken cancellationToken);

    /// <summary>Mensajes con secuencia posterior a <paramref name="afterSeq"/>, en orden.</summary>
    Task<IReadOnlyList<Message>> List(Guid conversationId, long afterSeq, int take, CancellationToken cancellationToken);

    Task<Message?> Find(Guid conversationId, Guid messageId, CancellationToken cancellationToken);

    Task<Message?> Last(Guid conversationId, CancellationToken cancellationToken);

    /// <summary>Mensajes posteriores a la secuencia dada que no envió esa cuenta (los avisos del sistema cuentan).</summary>
    Task<long> CountUnread(Guid conversationId, long afterSeq, Guid exceptSender, CancellationToken cancellationToken);

    Task<long> DeleteByConversations(IReadOnlyCollection<Guid> conversationIds, CancellationToken cancellationToken);
}

// ---------- Resultados ----------

public sealed record FollowupNoteResult(DateTime AtUtc, string Text);

public sealed record FollowupResult(Guid Id, Guid ConsultationId, Guid PetId, string PetName, FollowupStatus Status, DateOnly? NextReviewOn, bool IsOverdue, IReadOnlyList<FollowupNoteResult> Notes, Guid? ConversationId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static FollowupResult From(Followup f, DateOnly today, Guid? conversationId = null)
    {
        ArgumentNullException.ThrowIfNull(f);
        return new(f.Id, f.ConsultationId, f.PetId, f.PetName, f.Status, f.NextReviewOn, f.IsOverdue(today), f.Notes.Select(n => new FollowupNoteResult(n.AtUtc, n.Text)).ToArray(), conversationId, f.CreatedAtUtc, f.UpdatedAtUtc);
    }
}

public sealed record MessageResult(Guid Id, Guid ConversationId, long Seq, MessageKind Kind, bool IsMine, string Text, DateTime CreatedAtUtc)
{
    public static MessageResult From(Message m, Guid accountId)
    {
        ArgumentNullException.ThrowIfNull(m);
        return new(m.Id, m.ConversationId, m.Seq, m.Kind, m.SenderAccountId == accountId, m.Text, m.CreatedAtUtc);
    }
}

public sealed record MessageList(IReadOnlyList<MessageResult> Items, bool HasMore);

public sealed record ConversationSummary(Guid Id, Guid FollowupId, bool IsClosed, MessageResult? LastMessage, long UnreadCount, DateTime UpdatedAtUtc);

// ---------- Seguimiento (pm-138 a pm-142) ----------

public sealed record CreateFollowupCommand(Guid AccountId, Guid ConsultationId, int? ReviewInDays) : ICommand<AccessOutcome<FollowupResult>>;

public sealed record UpdateFollowupCommand(Guid AccountId, Guid FollowupId, FollowupStatus? Status, int? ReviewInDays) : ICommand<AccessOutcome<FollowupResult>>;

public sealed record AddFollowupNoteCommand(Guid AccountId, Guid FollowupId, string Text) : ICommand<AccessOutcome<FollowupResult>>;

public sealed record GetFollowupByConsultationQuery(Guid OwnerId, Guid ConsultationId) : IQuery<AccessOutcome<FollowupResult>>;

public sealed record ListFollowupsQuery(Guid AccountId, DateOnly? ReviewOn, bool OnlyActive, int? Page, int? PageSize) : IQuery<AccessOutcome<PagedResult<FollowupResult>>>;

/// <summary>
/// La veterinaria autora de la consulta abre el seguimiento (En tratamiento, con revisión opcional de 3, 7 o 15 días). Una consulta
/// tiene un solo seguimiento y al abrirlo se crea su conversación con el dueño y la veterinaria (pm-143).
/// </summary>
public sealed class CreateFollowupHandler(
    IBusinessRepository businesses, IConsultationRepository consultations, IPetRepository pets, IFollowupRepository followups,
    IConversationRepository conversations, IMessageRepository messages, TimeProvider timeProvider)
    : IRequestHandler<CreateFollowupCommand, AccessOutcome<FollowupResult>>
{
    public async Task<AccessOutcome<FollowupResult>> Handle(CreateFollowupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NoBusiness);
        }

        var consultation = await consultations.FindByIdForBusiness(request.ConsultationId, business.Id, cancellationToken).ConfigureAwait(false);
        var pet = consultation is null ? null : await pets.FindOwned(consultation.PetId, consultation.OwnerId, cancellationToken).ConfigureAwait(false);
        if (consultation is null || pet is null)
        {
            // Solo la veterinaria autora: una consulta de otra veterinaria o inexistente responde igual.
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var followup = Followup.Open(consultation.Id, pet.Id, pet.Name, consultation.OwnerId, business.Id, request.ReviewInDays, DateOnly.FromDateTime(now), now);
        if (!await followups.TryAdd(followup, cancellationToken).ConfigureAwait(false))
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.Conflict);
        }

        var conversation = Conversation.Open(followup.Id, pet.Id, consultation.OwnerId, business.Id, business.OwnerId, now);
        Guid? conversationId = null;
        if (await conversations.TryAdd(conversation, cancellationToken).ConfigureAwait(false))
        {
            conversationId = conversation.Id;
            var seq = await conversations.NextSeq(conversation.Id, now, cancellationToken).ConfigureAwait(false);
            await messages.Add(Message.FromSystem(conversation.Id, seq, "Se abrió el seguimiento de la consulta.", now), cancellationToken).ConfigureAwait(false);
        }

        return AccessOutcome.Ok(FollowupResult.From(followup, DateOnly.FromDateTime(now), conversationId));
    }
}

public sealed class UpdateFollowupHandler(
    IBusinessRepository businesses, IFollowupRepository followups, IConversationRepository conversations, IMessageRepository messages, TimeProvider timeProvider)
    : IRequestHandler<UpdateFollowupCommand, AccessOutcome<FollowupResult>>
{
    public async Task<AccessOutcome<FollowupResult>> Handle(UpdateFollowupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NoBusiness);
        }

        var followup = await followups.FindForBusiness(request.FollowupId, business.Id, cancellationToken).ConfigureAwait(false);
        if (followup is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var before = followup.Status;
        followup.Update(request.Status!.Value, request.ReviewInDays, today, now);
        if (!await followups.Update(followup, cancellationToken).ConfigureAwait(false))
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        var conversation = await conversations.FindByFollowup(followup.Id, cancellationToken).ConfigureAwait(false);
        if (conversation is not null && before != followup.Status)
        {
            // Aviso del sistema en la conversación (pm-145#CA-03). Dar de alta la cierra (pm-147#CA-01).
            var text = followup.Status switch
            {
                FollowupStatus.Discharged => "El seguimiento terminó: la mascota fue dada de alta.",
                FollowupStatus.Improving => "El seguimiento cambió a «Mejorando».",
                _ => "El seguimiento cambió a «En tratamiento».",
            };
            var seq = await conversations.NextSeq(conversation.Id, now, cancellationToken).ConfigureAwait(false);
            await messages.Add(Message.FromSystem(conversation.Id, seq, text, now), cancellationToken).ConfigureAwait(false);
            if (followup.Status == FollowupStatus.Discharged)
            {
                await conversations.SetClosed(conversation.Id, true, now, cancellationToken).ConfigureAwait(false);
            }
        }

        return AccessOutcome.Ok(FollowupResult.From(followup, today, conversation?.Id));
    }
}

public sealed class AddFollowupNoteHandler(IBusinessRepository businesses, IFollowupRepository followups, TimeProvider timeProvider)
    : IRequestHandler<AddFollowupNoteCommand, AccessOutcome<FollowupResult>>
{
    public async Task<AccessOutcome<FollowupResult>> Handle(AddFollowupNoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NoBusiness);
        }

        var followup = await followups.FindForBusiness(request.FollowupId, business.Id, cancellationToken).ConfigureAwait(false);
        if (followup is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        if (followup.Notes.Count >= Followup.MaxNotes)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.Conflict);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        followup.AddNote(request.Text, now);
        return await followups.Update(followup, cancellationToken).ConfigureAwait(false)
            ? AccessOutcome.Ok(FollowupResult.From(followup, DateOnly.FromDateTime(now)))
            : AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
    }
}

/// <summary>El dueño ve estado, próxima revisión y notas del seguimiento de su consulta; de solo lectura (pm-141).</summary>
public sealed class GetFollowupByConsultationHandler(IFollowupRepository followups, IConversationRepository conversations, TimeProvider timeProvider)
    : IRequestHandler<GetFollowupByConsultationQuery, AccessOutcome<FollowupResult>>
{
    public async Task<AccessOutcome<FollowupResult>> Handle(GetFollowupByConsultationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var followup = await followups.FindByConsultation(request.ConsultationId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (followup is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        var conversation = await conversations.FindByFollowup(followup.Id, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(FollowupResult.From(followup, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime), conversation?.Id));
    }
}

public sealed class ListFollowupsHandler(IBusinessRepository businesses, IFollowupRepository followups, TimeProvider timeProvider)
    : IRequestHandler<ListFollowupsQuery, AccessOutcome<PagedResult<FollowupResult>>>
{
    public async Task<AccessOutcome<PagedResult<FollowupResult>>> Handle(ListFollowupsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<PagedResult<FollowupResult>>(AccessStatus.NoBusiness);
        }

        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var total = await followups.CountByBusiness(business.Id, request.ReviewOn, request.OnlyActive, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (skip >= total || skip > int.MaxValue)
        {
            return AccessOutcome.Ok(new PagedResult<FollowupResult>([], page, pageSize, total));
        }

        var items = await followups.ListByBusiness(business.Id, request.ReviewOn, request.OnlyActive, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(new PagedResult<FollowupResult>(items.Select(f => FollowupResult.From(f, today)).ToArray(), page, pageSize, total));
    }
}

public sealed class CreateFollowupCommandValidator : IRequestValidator<CreateFollowupCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateFollowupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.ConsultationId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("consultationId", "La consulta es obligatoria."));
        }

        if (!Followup.IsValidReviewDays(request.ReviewInDays))
        {
            failures.Add(new ValidationFailure("nextReviewInDays", "La próxima revisión debe ser en 3, 7 o 15 días."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class UpdateFollowupCommandValidator : IRequestValidator<UpdateFollowupCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateFollowupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.Status is not { } status || !Enum.IsDefined(status))
        {
            failures.Add(new ValidationFailure("status", "El estado debe ser Treating, Improving o Discharged."));
        }

        if (!Followup.IsValidReviewDays(request.ReviewInDays))
        {
            failures.Add(new ValidationFailure("nextReviewInDays", "La próxima revisión debe ser en 3, 7 o 15 días."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class AddFollowupNoteCommandValidator : IRequestValidator<AddFollowupNoteCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(AddFollowupNoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.FollowupId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("followupId", "El seguimiento es obligatorio."));
        }

        var text = request.Text?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > Followup.MaxNoteLength)
        {
            failures.Add(new ValidationFailure("text", $"La nota es obligatoria y debe tener de 1 a {Followup.MaxNoteLength} caracteres."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class ListFollowupsQueryValidator : IRequestValidator<ListFollowupsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListFollowupsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PaginationConvention.Check(request.Page, request.PageSize));
    }
}

// ---------- Chat (pm-144 a pm-149) ----------

public sealed record SendMessageCommand(Guid AccountId, Guid ConversationId, string Text) : ICommand<AccessOutcome<MessageResult>>;

public sealed record ListMessagesQuery(Guid AccountId, Guid ConversationId, Guid? AfterMessageId, int? PageSize) : IQuery<AccessOutcome<MessageList>>;

public sealed record ListConversationsQuery(Guid AccountId, int? Page, int? PageSize) : IQuery<PagedResult<ConversationSummary>>;

public sealed record MarkReadCommand(Guid AccountId, Guid ConversationId, Guid? UpToMessageId) : ICommand<AccessOutcome<ConversationSummary>>;

public sealed record ReopenConversationCommand(Guid AccountId, Guid ConversationId) : ICommand<AccessOutcome<FollowupResult>>;

public sealed class SendMessageHandler(IConversationRepository conversations, IMessageRepository messages, TimeProvider timeProvider)
    : IRequestHandler<SendMessageCommand, AccessOutcome<MessageResult>>
{
    public async Task<AccessOutcome<MessageResult>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conversation = await conversations.FindById(request.ConversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null || !conversation.HasParticipant(request.AccountId))
        {
            // Quien no es participante recibe 404, igual que una conversación inexistente (pm-144#CA-02).
            return AccessOutcome.Failed<MessageResult>(AccessStatus.NotFound);
        }

        if (conversation.IsClosed)
        {
            return AccessOutcome.Failed<MessageResult>(AccessStatus.Closed);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var seq = await conversations.NextSeq(conversation.Id, now, cancellationToken).ConfigureAwait(false);
        var message = Message.FromParticipant(conversation.Id, seq, request.AccountId, request.Text, now);
        await messages.Add(message, cancellationToken).ConfigureAwait(false);
        await conversations.MarkRead(conversation.Id, conversation.IsBusinessSide(request.AccountId), seq, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(MessageResult.From(message, request.AccountId));
    }
}

public sealed class ListMessagesHandler(IConversationRepository conversations, IMessageRepository messages)
    : IRequestHandler<ListMessagesQuery, AccessOutcome<MessageList>>
{
    public async Task<AccessOutcome<MessageList>> Handle(ListMessagesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conversation = await conversations.FindById(request.ConversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null || !conversation.HasParticipant(request.AccountId))
        {
            return AccessOutcome.Failed<MessageList>(AccessStatus.NotFound);
        }

        long after = 0;
        if (request.AfterMessageId is { } afterId)
        {
            var anchor = await messages.Find(conversation.Id, afterId, cancellationToken).ConfigureAwait(false);
            if (anchor is null)
            {
                return AccessOutcome.Failed<MessageList>(AccessStatus.NotFound);
            }

            after = anchor.Seq;
        }

        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var found = await messages.List(conversation.Id, after, pageSize + 1, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(new MessageList(found.Take(pageSize).Select(m => MessageResult.From(m, request.AccountId)).ToArray(), found.Count > pageSize));
    }
}

public sealed class ListConversationsHandler(IConversationRepository conversations, IMessageRepository messages)
    : IRequestHandler<ListConversationsQuery, PagedResult<ConversationSummary>>
{
    public async Task<PagedResult<ConversationSummary>> Handle(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var total = await conversations.CountForAccount(request.AccountId, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return new PagedResult<ConversationSummary>([], page, pageSize, total);
        }

        var items = new List<ConversationSummary>();
        foreach (var conversation in await conversations.ListForAccount(request.AccountId, (int)skip, pageSize, cancellationToken).ConfigureAwait(false))
        {
            items.Add(await Summarize(conversation, request.AccountId, conversations, messages, cancellationToken).ConfigureAwait(false));
        }

        return new PagedResult<ConversationSummary>(items, page, pageSize, total);
    }

    internal static async Task<ConversationSummary> Summarize(Conversation conversation, Guid accountId, IConversationRepository conversations, IMessageRepository messages, CancellationToken cancellationToken)
    {
        // Se relee la conversación para informar el estado de lectura más reciente.
        var fresh = await conversations.FindById(conversation.Id, cancellationToken).ConfigureAwait(false) ?? conversation;
        var last = await messages.Last(fresh.Id, cancellationToken).ConfigureAwait(false);
        var unread = await messages.CountUnread(fresh.Id, fresh.ReadSeqOf(accountId), accountId, cancellationToken).ConfigureAwait(false);
        return new ConversationSummary(fresh.Id, fresh.FollowupId, fresh.IsClosed, last is null ? null : MessageResult.From(last, accountId), unread, fresh.UpdatedAtUtc);
    }
}

public sealed class MarkReadHandler(IConversationRepository conversations, IMessageRepository messages)
    : IRequestHandler<MarkReadCommand, AccessOutcome<ConversationSummary>>
{
    public async Task<AccessOutcome<ConversationSummary>> Handle(MarkReadCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conversation = await conversations.FindById(request.ConversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null || !conversation.HasParticipant(request.AccountId))
        {
            return AccessOutcome.Failed<ConversationSummary>(AccessStatus.NotFound);
        }

        var upTo = conversation.LastSeq;
        if (request.UpToMessageId is { } messageId)
        {
            var anchor = await messages.Find(conversation.Id, messageId, cancellationToken).ConfigureAwait(false);
            if (anchor is null)
            {
                return AccessOutcome.Failed<ConversationSummary>(AccessStatus.NotFound);
            }

            upTo = anchor.Seq;
        }

        // Solo cambia el lado de quien lo pide: el estado del otro participante no se toca (pm-149#CA-02).
        await conversations.MarkRead(conversation.Id, conversation.IsBusinessSide(request.AccountId), upTo, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(await ListConversationsHandler.Summarize(conversation, request.AccountId, conversations, messages, cancellationToken).ConfigureAwait(false));
    }
}

/// <summary>La veterinaria reabre la conversación y el seguimiento vuelve a «En tratamiento» (pm-147). El dueño no puede reabrirla.</summary>
public sealed class ReopenConversationHandler(
    IConversationRepository conversations, IFollowupRepository followups, IMessageRepository messages, TimeProvider timeProvider)
    : IRequestHandler<ReopenConversationCommand, AccessOutcome<FollowupResult>>
{
    public async Task<AccessOutcome<FollowupResult>> Handle(ReopenConversationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conversation = await conversations.FindById(request.ConversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null || !conversation.HasParticipant(request.AccountId))
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        if (!conversation.IsBusinessSide(request.AccountId))
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.Forbidden);
        }

        var followup = await followups.FindById(conversation.FollowupId, cancellationToken).ConfigureAwait(false);
        if (followup is null)
        {
            return AccessOutcome.Failed<FollowupResult>(AccessStatus.NotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (conversation.IsClosed)
        {
            followup.Reopen(now);
            await followups.Update(followup, cancellationToken).ConfigureAwait(false);
            await conversations.SetClosed(conversation.Id, false, now, cancellationToken).ConfigureAwait(false);
            var seq = await conversations.NextSeq(conversation.Id, now, cancellationToken).ConfigureAwait(false);
            await messages.Add(Message.FromSystem(conversation.Id, seq, "La veterinaria reabrió el seguimiento.", now), cancellationToken).ConfigureAwait(false);
        }

        return AccessOutcome.Ok(FollowupResult.From(followup, DateOnly.FromDateTime(now), conversation.Id));
    }
}

public sealed class SendMessageCommandValidator : IRequestValidator<SendMessageCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(SendMessageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.ConversationId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("conversationId", "La conversación es obligatoria."));
        }

        if (!Message.IsValidText(request.Text))
        {
            failures.Add(new ValidationFailure("text", $"El mensaje es obligatorio y debe tener de 1 a {Message.MaxTextLength} caracteres."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class ListMessagesQueryValidator : IRequestValidator<ListMessagesQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListMessagesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.ConversationId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("conversationId", "La conversación es obligatoria."));
        }

        failures.AddRange(PaginationConvention.Check(1, request.PageSize));
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class ListConversationsQueryValidator : IRequestValidator<ListConversationsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PaginationConvention.Check(request.Page, request.PageSize));
    }
}

public sealed class MarkReadCommandValidator : IRequestValidator<MarkReadCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(MarkReadCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(request.ConversationId == Guid.Empty ? [new ValidationFailure("conversationId", "La conversación es obligatoria.")] : []);
    }
}

public sealed class ReopenConversationCommandValidator : IRequestValidator<ReopenConversationCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ReopenConversationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(request.ConversationId == Guid.Empty ? [new ValidationFailure("conversationId", "La conversación es obligatoria.")] : []);
    }
}
