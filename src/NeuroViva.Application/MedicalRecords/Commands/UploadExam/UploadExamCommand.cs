using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadExam;

public sealed record UploadExamCommand(
    Guid? PatientId,
    string Description,
    DateTime? EventDate,
    IReadOnlyList<AttachmentInput> Attachments)
    : IRequest<Result<UploadExamResult>>;
