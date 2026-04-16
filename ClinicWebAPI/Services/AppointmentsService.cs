using ClinicWebAPI.DTOs;
using Microsoft.Data.SqlClient;
namespace ClinicWebAPI.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly string _connectionString;

    public AppointmentsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? "";
    }
    public async Task<AppointmentListDto> GetAllAppointmentsAsync()
    {
        string query = "SELECT * FROM Appointments";
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;

        var appointmentListDto = new AppointmentListDto()
        {
            AppointmentDetails = new List<AppointmentDetailsDto>()
        };

        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var dto = new AppointmentDetailsDto
            {
                IdAppointment = reader.GetInt32(0),
                Status = reader.GetString(1)
            };
            appointmentListDto.AppointmentDetails.Add(dto);
        }
        return appointmentListDto;
    }
}