using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadClinicalNote;

public sealed record UploadClinicalNoteCommand(
    Guid? PatientId,
    string EventType,
    string Description,
    DateTime? EventDate,
    IReadOnlyList<AttachmentInput> Attachments)
    : IRequest<Result<UploadClinicalNoteResult>>;
