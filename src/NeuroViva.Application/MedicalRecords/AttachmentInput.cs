namespace NeuroViva.Application.MedicalRecords;

public sealed record AttachmentInput(byte[] Bytes, string FileName, string ContentType);
