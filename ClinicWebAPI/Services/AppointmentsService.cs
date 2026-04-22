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
    public async Task<AppointmentListDto> GetAllAppointmentsAsync(string? status, string? patientLastName)
    {
        var query = @"
            SELECT a.IdAppointment, a.AppointmentDate, a.Status, a.Reason, 
                   p.FirstName + ' ' + p.LastName as PatientFullName, p.Email
            FROM Appointments a
            INNER JOIN Patients p ON a.IdPatient = p.IdPatient
            WHERE (@Status IS NULL OR a.Status = @Status)
              AND (@LastName IS NULL OR p.LastName = @LastName)
            ORDER BY a.AppointmentDate ASC";

        var result = new AppointmentListDto()
        {
            AppointmentDetails = new List<AppointmentDetailsDto>()
        };
        
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastName", (object?)patientLastName ?? DBNull.Value);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.AppointmentDetails.Add(new AppointmentDetailsDto
            {
                IdAppointment = reader.GetInt32(0),
                AppointmentDate = reader.GetDateTime(1),
                Status = reader.GetString(2),
                Reason = reader.GetString(3),
                PatientFullName = reader.GetString(4),
                PatientEmail = reader.GetString(5)
            });
        }
        return result;
    }
}