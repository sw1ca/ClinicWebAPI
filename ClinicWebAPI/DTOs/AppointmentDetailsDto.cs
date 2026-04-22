namespace ClinicWebAPI.DTOs;

public class AppointmentDetailsDto
{
    public int IdAppointment { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string? InternalNotes { get; set; }
    
    public string PatientFullName { get; set; } = null!;
    public string PatientEmail { get; set; } = null!;
    public string PatientPhoneNumber { get; set; } = null!;
    
    public string DoctorFullName { get; set; } = null!;
    public string DoctorLicenseNumber { get; set; } = null!;
    public string SpecializationName { get; set; } = null!;
}