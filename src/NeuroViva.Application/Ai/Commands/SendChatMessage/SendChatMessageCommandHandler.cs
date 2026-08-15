using MediatR;
using NeuroViva.Application.Ai.Queries;
using NeuroViva.Application.Ai.Services;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Ai.Commands.SendChatMessage;

public sealed class SendChatMessageCommandHandler
    : IRequestHandler<SendChatMessageCommand, Result<ChatMessageDto>>
{
    private readonly IPatientAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IPatientContextReadRepository _patientContextRepo;
    private readonly IAiChatConversationRepository _conversationRepo;
    private readonly IAiChatMessageRepository _messageRepo;
    private readonly IPatientContextBuilder _contextBuilder;
    private readonly IGroqChatService _groqService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public SendChatMessageCommandHandler(
        IPatientAccessGuard accessGuard,
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepo,
        IPatientContextReadRepository patientContextRepo,
        IAiChatConversationRepository conversationRepo,
        IAiChatMessageRepository messageRepo,
        IPatientContextBuilder contextBuilder,
        IGroqChatService groqService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _accessGuard = accessGuard;
        _currentUser = currentUser;
        _doctorRepo = doctorRepo;
        _patientContextRepo = patientContextRepo;
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _contextBuilder = contextBuilder;
        _groqService = groqService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ChatMessageDto>> Handle(
        SendChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        // Step 1: Access guard
        var accessResult = await _accessGuard.ResolveAndAuthorizeAsync(command.PatientId, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult.Error;

        // Step 2: Resolve current doctor
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor profile not found.");

        // Step 3: Get or create the active conversation
        var conversation = await _conversationRepo.GetActiveByDoctorAndPatientAsync(
            doctor.Id, command.PatientId, cancellationToken);

        if (conversation is null)
        {
            // Need TenantId from patient (doctors are cross-tenant)
            var profile = await _patientContextRepo.GetPatientProfileAsync(command.PatientId, cancellationToken);
            if (profile is null)
                return Error.NotFound("patient.not_found", "Patient not found.");

            var now = _clock.UtcNow;
            conversation = AiChatConversation.Create(doctor.Id, command.PatientId, profile.TenantId, now);
            await _conversationRepo.AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var convId = conversation.Id;
        var messageTime = _clock.UtcNow;

        // Step 4: Persist the user message (not yet committed)
        var userMessage = AiChatMessage.Create(convId, AiChatRole.User, command.Message, messageTime);
        await _messageRepo.AddAsync(userMessage, cancellationToken);

        // Step 5: Build system prompt from current patient context
        var promptResult = await _contextBuilder.BuildSystemPromptAsync(command.PatientId, cancellationToken);
        if (promptResult.IsFailure)
            return promptResult.Error;

        // Step 6: Load previous conversation history from DB and compose Groq messages
        var history = await _messageRepo.ListByConversationOrderedAsync(convId, cancellationToken);

        var groqMessages = new List<GroqChatMessage>
        {
            new GroqChatMessage("system", promptResult.Value)
        };

        // Previous turns (already persisted) — history does NOT include the unsaved userMessage yet
        foreach (var m in history)
            groqMessages.Add(new GroqChatMessage(m.Role == AiChatRole.User ? "user" : "assistant", m.Content));

        // Append the current user turn
        groqMessages.Add(new GroqChatMessage("user", command.Message));

        // Step 7: Call Groq
        var groqResult = await _groqService.CompleteAsync(groqMessages, cancellationToken);
        if (groqResult.IsFailure)
            return groqResult.Error;

        var replyContent = groqResult.Value;
        var replyTime = _clock.UtcNow;

        // Step 8: Persist the assistant message
        var assistantMessage = AiChatMessage.Create(convId, AiChatRole.Assistant, replyContent, replyTime);
        await _messageRepo.AddAsync(assistantMessage, cancellationToken);

        // Step 9: Touch the conversation's last message timestamp
        conversation.TouchLastMessage(replyTime);
        _conversationRepo.Update(conversation);

        // Step 10: Commit user message + assistant message + conversation update
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChatMessageDto>.Success(new ChatMessageDto(
            Id: assistantMessage.Id,
            Role: "assistant",
            Content: replyContent,
            CreatedAt: replyTime));
    }
}
