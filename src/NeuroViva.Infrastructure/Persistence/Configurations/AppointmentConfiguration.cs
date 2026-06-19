using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Appointments;
using NeuroViva.Domain.Appointments.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    // cita_estado_check: programada, confirmada, realizada, cancelada
    private static string StatusToDb(AppointmentStatus v)
    {
        if (v == AppointmentStatus.Scheduled)  return "programada";
        if (v == AppointmentStatus.Confirmed)  return "confirmada";
        if (v == AppointmentStatus.Completed)  return "realizada";
        if (v == AppointmentStatus.Cancelled)  return "cancelada";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped AppointmentStatus value.");
    }

    private static AppointmentStatus StatusFromDb(string v)
    {
        if (v == "programada") return AppointmentStatus.Scheduled;
        if (v == "confirmada") return AppointmentStatus.Confirmed;
        if (v == "realizada")  return AppointmentStatus.Completed;
        if (v == "cancelada")  return AppointmentStatus.Cancelled;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown AppointmentStatus DB value.");
    }

    // cita_tipo_check: consulta, examen, procedimiento, teleconsulta
    private static string TypeToDb(AppointmentType v)
    {
        if (v == AppointmentType.Consultation)    return "consulta";
        if (v == AppointmentType.Exam)            return "examen";
        if (v == AppointmentType.Procedure)       return "procedimiento";
        if (v == AppointmentType.Teleconsultation) return "teleconsulta";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped AppointmentType value.");
    }

    private static AppointmentType TypeFromDb(string v)
    {
        if (v == "consulta")      return AppointmentType.Consultation;
        if (v == "examen")        return AppointmentType.Exam;
        if (v == "procedimiento") return AppointmentType.Procedure;
        if (v == "teleconsulta")  return AppointmentType.Teleconsultation;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown AppointmentType DB value.");
    }

    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointment");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.DoctorId).HasColumnName("doctor_id").IsRequired(false);
        builder.Property(a => a.Type).HasColumnName("type")
            .HasConversion(v => TypeToDb(v), v => TypeFromDb(v));
        builder.Property(a => a.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(a => a.Status).HasColumnName("status")
            .HasConversion(v => StatusToDb(v), v => StatusFromDb(v));
        builder.Property(a => a.Notes).HasColumnName("notes");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Ignore(a => a.DomainEvents);
    }
}
