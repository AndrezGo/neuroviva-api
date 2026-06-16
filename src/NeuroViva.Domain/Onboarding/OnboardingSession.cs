using System.Text.Json;
using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Onboarding;

public sealed class OnboardingSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = default!;
    public int CurrentStep { get; private set; }
    public int TotalSteps { get; private set; }
    public bool Completed { get; private set; }
    public JsonDocument Answers { get; private set; } = default!;
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    private OnboardingSession() { }
    public static OnboardingSession Start(Guid userId, string role, int totalSteps) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, Role = role, CurrentStep = 1, TotalSteps = totalSteps,
        Completed = false, Answers = JsonDocument.Parse("{}"), StartedAt = DateTime.UtcNow
    };
    public void Complete() { Completed = true; CompletedAt = DateTime.UtcNow; }
    public void AdvanceStep() { if (CurrentStep < TotalSteps) CurrentStep++; }
}
