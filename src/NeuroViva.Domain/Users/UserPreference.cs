using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Users;

public sealed class UserPreference : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public bool LargeText { get; private set; }
    public bool HighContrast { get; private set; }
    public bool NotifyMedications { get; private set; }
    public bool NotifyAppointments { get; private set; }
    public string Language { get; private set; } = "es";
    public DateTime UpdatedAt { get; private set; }
    private UserPreference() { }
    public static UserPreference CreateDefault(Guid userId) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, LargeText = false, HighContrast = false,
        NotifyMedications = true, NotifyAppointments = true, Language = "es", UpdatedAt = DateTime.UtcNow
    };
    public void Update(bool largeText, bool highContrast, bool notifyMeds, bool notifyAppts, string language)
    {
        LargeText = largeText; HighContrast = highContrast;
        NotifyMedications = notifyMeds; NotifyAppointments = notifyAppts;
        Language = language; UpdatedAt = DateTime.UtcNow;
    }
}
