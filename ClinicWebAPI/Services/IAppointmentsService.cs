using ClinicWebAPI.DTOs;

namespace ClinicWebAPI.Services;

public interface IAppointmentsService
{
    Task<AppointmentListDto> GetAllAppointmentsAsync(string? status, string? patientLastName);
    Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int id);
    Task<int> CreateAppointmentAsync(CreateAppointmentDto createDto);
    Task<bool?> UpdateAppointmentAsync(int id, UpdateAppointmentDto updateDto);
    Task<bool?> DeleteAppointmentAsync(int id);
}