using FluentValidation;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadClinicalNote;

public sealed class UploadClinicalNoteCommandValidator : AbstractValidator<UploadClinicalNoteCommand>
{
    private static readonly HashSet<string> ValidEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "consultation",
        "note",
        "other"
    };

    public UploadClinicalNoteCommandValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .Must(t => ValidEventTypes.Contains(t))
            .WithMessage("eventType must be one of: consultation, note, other.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Attachments)
            .NotNull()
            .Must(list => list.Count <= AttachmentValidation.MaxAttachmentsPerRecord)
            .WithMessage($"A maximum of {AttachmentValidation.MaxAttachmentsPerRecord} attachments are allowed per record.");
    }
}
