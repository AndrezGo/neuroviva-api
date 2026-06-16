using System.Text.Json;
using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Onboarding;

public sealed class OnboardingStep : Entity<Guid>
{
    public string Role { get; private set; } = default!;
    public int OrderNum { get; private set; }
    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public JsonDocument? Options { get; private set; }
    public bool Skippable { get; private set; }
    private OnboardingStep() { }
}
