using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Appointments;
using NeuroViva.Domain.Appointments.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointment");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.DoctorId).HasColumnName("doctor_id");
        builder.Property(a => a.Type).HasColumnName("type")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<AppointmentType>(v, true));
        builder.Property(a => a.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(a => a.Status).HasColumnName("status")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<AppointmentStatus>(v, true));
        builder.Property(a => a.Notes).HasColumnName("notes");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Ignore(a => a.DomainEvents);
    }
}
