using ClinicWebAPI.DTOs;

namespace ClinicWebAPI.Services;

public interface IAppointmentsService
{
    Task<AppointmentListDto> GetAllAppointmentsAsync();
}