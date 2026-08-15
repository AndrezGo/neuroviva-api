using System.Text;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.MedicalRecords;

namespace NeuroViva.Application.Ai.Services;

public sealed class PatientContextBuilder : IPatientContextBuilder
{
    private const int ExamLimit = 25;
    private const int NoteLimit = 25;
    private const int FollowUpLimit = 30;

    private readonly IPatientContextReadRepository _profileRepo;
    private readonly IMedicalRecordReadRepository _medicalRepo;

    public PatientContextBuilder(
        IPatientContextReadRepository profileRepo,
        IMedicalRecordReadRepository medicalRepo)
    {
        _profileRepo = profileRepo;
        _medicalRepo = medicalRepo;
    }

    public async Task<Result<string>> BuildSystemPromptAsync(Guid patientId, CancellationToken ct)
    {
        var profile = await _profileRepo.GetPatientProfileAsync(patientId, ct);
        if (profile is null)
            return Error.NotFound("patient.not_found", $"Paciente {patientId} no encontrado.");

        var exams = await _medicalRepo.ListExamsTextAsync(patientId, ExamLimit, ct);
        var notes = await _medicalRepo.ListClinicalNotesTextAsync(patientId, NoteLimit, ct);
        var followUps = await _medicalRepo.ListFollowUpTextAsync(patientId, FollowUpLimit, ct);

        var sb = new StringBuilder();

        sb.AppendLine("Eres un asistente clínico de IA para NeuroViva. Apoyas al médico tratante con sugerencias basadas en la información clínica del paciente. Tus respuestas NO son diagnóstico definitivo, son sugerencias que el médico debe validar profesionalmente.");
        sb.AppendLine();
        sb.AppendLine("PACIENTE:");
        sb.AppendLine($"- Nombre: {profile.Name}");
        sb.AppendLine($"- Edad: {profile.Age} años");
        sb.AppendLine($"- Condiciones: {(profile.Conditions.Length > 0 ? string.Join(", ", profile.Conditions) : "no registradas")}");
        sb.AppendLine();

        sb.AppendLine($"EXÁMENES RECIENTES (últimos {ExamLimit}, más reciente primero):");
        if (exams.Count == 0)
        {
            sb.AppendLine("- Sin exámenes registrados.");
        }
        else
        {
            foreach (var e in exams)
                sb.AppendLine($"- [{e.EventDate:yyyy-MM-dd}] {e.Description}");
        }
        sb.AppendLine();

        sb.AppendLine($"NOTAS CLÍNICAS RECIENTES (últimos {NoteLimit}):");
        if (notes.Count == 0)
        {
            sb.AppendLine("- Sin notas clínicas registradas.");
        }
        else
        {
            foreach (var n in notes)
                sb.AppendLine($"- [{n.EventDate:yyyy-MM-dd}] {n.EventType}: {n.Description}");
        }
        sb.AppendLine();

        sb.AppendLine($"SEGUIMIENTO RECIENTE (últimos {FollowUpLimit}):");
        if (followUps.Count == 0)
        {
            sb.AppendLine("- Sin seguimiento registrado.");
        }
        else
        {
            foreach (var f in followUps)
            {
                var line = $"- [{f.EventDate:yyyy-MM-dd}] {f.Type} — {f.Title}";
                if (!string.IsNullOrWhiteSpace(f.Status))
                    line += $" | {f.Status}";
                if (!string.IsNullOrWhiteSpace(f.Description))
                    line += $" | {f.Description}";
                sb.AppendLine(line);
            }
        }
        sb.AppendLine();

        sb.Append("Responde en español, de forma concisa y clínica. Si te preguntan algo fuera del alcance clínico o no relacionado con este paciente, redirige amablemente.");

        return sb.ToString();
    }
}
