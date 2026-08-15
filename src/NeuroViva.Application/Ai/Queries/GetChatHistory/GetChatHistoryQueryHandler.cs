using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Ai.Queries.GetChatHistory;

public sealed class GetChatHistoryQueryHandler
    : IRequestHandler<GetChatHistoryQuery, Result<IReadOnlyList<ChatMessageDto>>>
{
    private readonly IPatientAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IAiChatConversationRepository _conversationRepo;
    private readonly IAiChatMessageRepository _messageRepo;

    public GetChatHistoryQueryHandler(
        IPatientAccessGuard accessGuard,
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepo,
        IAiChatConversationRepository conversationRepo,
        IAiChatMessageRepository messageRepo)
    {
        _accessGuard = accessGuard;
        _currentUser = currentUser;
        _doctorRepo = doctorRepo;
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
    }

    public async Task<Result<IReadOnlyList<ChatMessageDto>>> Handle(
        GetChatHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var accessResult = await _accessGuard.ResolveAndAuthorizeAsync(request.PatientId, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult.Error;

        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor profile not found.");

        var conversation = await _conversationRepo.GetActiveByDoctorAndPatientAsync(
            doctor.Id, request.PatientId, cancellationToken);

        if (conversation is null)
        {
            // No conversation yet — return empty list (not an error).
            return Result<IReadOnlyList<ChatMessageDto>>.Success(Array.Empty<ChatMessageDto>());
        }

        var messages = await _messageRepo.ListByConversationOrderedAsync(conversation.Id, cancellationToken);

        var dtos = messages
            .Select(m => new ChatMessageDto(
                Id: m.Id,
                Role: m.Role == AiChatRole.User ? "user" : "assistant",
                Content: m.Content,
                CreatedAt: m.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<ChatMessageDto>>.Success(dtos);
    }
}
