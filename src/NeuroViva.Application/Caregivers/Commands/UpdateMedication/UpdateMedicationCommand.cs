using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.UpdateMedication;

public sealed record UpdateMedicationCommand(
    Guid MedicationId,
    string Name,
    string Dose,
    string Frequency,
    // Required ISO date string (yyyy-MM-dd).
    string StartDate,
    // Optional ISO date string (yyyy-MM-dd).
    string? EndDate,
    // Optional free-text name of the prescribing doctor.
    string? PrescribingDoctorName,
    // Optional free-text notes about the medication.
    string? Notes
) : IRequest<Result>;
