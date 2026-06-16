namespace NeuroViva.Application.Common.Authorization;

public static class Policies
{
    public const string DoctorOnly = "DoctorOnly";
    public const string CaregiverOnly = "CaregiverOnly";
    public const string PatientOnly = "PatientOnly";
    public const string CaregiverOrDoctor = "CaregiverOrDoctor";
    public const string AdminOnly = "AdminOnly";
    public const string ScientificCommittee = "ScientificCommittee";
    public const string Authenticated = "Authenticated";
}

public static class Roles
{
    public const string Patient = "paciente";
    public const string Caregiver = "cuidador";
    public const string Doctor = "medico";
    public const string ScientificCommittee = "comite_cientifico";
    public const string Admin = "admin";
}

public static class ClaimNames
{
    public const string InternalUserId = "internal_user_id";
    public const string TenantId = "tenant_id";
    public const string Sub = "sub";
}
