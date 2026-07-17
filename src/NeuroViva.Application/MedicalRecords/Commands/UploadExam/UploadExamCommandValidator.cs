using FluentValidation;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadExam;

public sealed class UploadExamCommandValidator : AbstractValidator<UploadExamCommand>
{
    public UploadExamCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Attachments)
            .NotNull()
            .Must(list => list.Count <= AttachmentValidation.MaxAttachmentsPerRecord)
            .WithMessage($"A maximum of {AttachmentValidation.MaxAttachmentsPerRecord} attachments are allowed per record.");
    }
}
