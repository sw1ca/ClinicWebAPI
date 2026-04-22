namespace ClinicWebAPI.DTOs;

public class UpdateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
}