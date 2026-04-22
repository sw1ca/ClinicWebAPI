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

    public async Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = @"SELECT a.IdAppointment, a.AppointmentDate, a.Status, a.Reason, a.InternalNotes,
                             p.FirstName + ' ' + p.LastName, p.Email, p.PhoneNumber,
                             d.FirstName + ' ' + d.LastName, d.LicenseNumber, s.Name
                      FROM Appointments a
                      JOIN Patients p ON a.IdPatient = p.IdPatient
                      JOIN Doctors d ON a.IdDoctor = d.IdDoctor
                      JOIN Specializations s ON d.IdSpecialization = s.IdSpecialization
                      WHERE a.IdAppointment = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new AppointmentDetailsDto {
                IdAppointment = reader.GetInt32(0),
                AppointmentDate = reader.GetDateTime(1),
                Status = reader.GetString(2),
                Reason = reader.GetString(3),
                InternalNotes = reader.IsDBNull(4) ? null : reader.GetString(4),
                PatientFullName = reader.GetString(5),
                PatientEmail = reader.GetString(6),
                PatientPhoneNumber = reader.GetString(7),
                DoctorFullName = reader.GetString(8),
                DoctorLicenseNumber = reader.GetString(9),
                SpecializationName = reader.GetString(10)
            };
        }
        return null;
    }

    public async Task<int> CreateAppointmentAsync(CreateAppointmentDto createDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var checkQuery = "SELECT COUNT(*) FROM Appointments WHERE IdDoctor = @IdDoc AND AppointmentDate = @Date";
        using var checkCmd = new SqlCommand(checkQuery, connection);
        checkCmd.Parameters.AddWithValue("@IdDoc", createDto.IdDoctor);
        checkCmd.Parameters.AddWithValue("@Date", createDto.AppointmentDate);
        
        int count = (int)await checkCmd.ExecuteScalarAsync()!;
        if (count > 0) throw new InvalidOperationException("Conflict: Doctor is busy at this time.");
        
        var insertQuery = @"INSERT INTO Appointments (IdPatient, IdDoctor, AppointmentDate, Status, Reason) 
                            OUTPUT INSERTED.IdAppointment
                            VALUES (@IdPat, @IdDoc, @Date, 'Scheduled', @Reason)";
        
        using var insertCmd = new SqlCommand(insertQuery, connection);
        insertCmd.Parameters.AddWithValue("@IdPat", createDto.IdPatient);
        insertCmd.Parameters.AddWithValue("@IdDoc", createDto.IdDoctor);
        insertCmd.Parameters.AddWithValue("@Date", createDto.AppointmentDate);
        insertCmd.Parameters.AddWithValue("@Reason", createDto.Reason);

        return (int)await insertCmd.ExecuteScalarAsync()!;
    }
    public async Task<bool?> UpdateAppointmentAsync(int id, UpdateAppointmentDto updateDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        
        var statusQuery = "SELECT Status FROM Appointments WHERE IdAppointment = @Id";
        using var statusCmd = new SqlCommand(statusQuery, connection);
        statusCmd.Parameters.AddWithValue("@Id", id);
        var currentStatus = await statusCmd.ExecuteScalarAsync() as string;

        if (currentStatus == null) return null;
        if (currentStatus == "Completed") return false;

        var updateQuery = @"UPDATE Appointments SET AppointmentDate = @Date, Reason = @Reason, Status = @Status 
                            WHERE IdAppointment = @Id";
        using var updateCmd = new SqlCommand(updateQuery, connection);
        updateCmd.Parameters.AddWithValue("@Date", updateDto.AppointmentDate);
        updateCmd.Parameters.AddWithValue("@Reason", updateDto.Reason);
        updateCmd.Parameters.AddWithValue("@Status", updateDto.Status);
        updateCmd.Parameters.AddWithValue("@Id", id);

        await updateCmd.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool?> DeleteAppointmentAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var checkQuery = "SELECT Status FROM Appointments WHERE IdAppointment = @Id";
        using var checkCmd = new SqlCommand(checkQuery, connection);
        checkCmd.Parameters.AddWithValue("@Id", id);
        var status = await checkCmd.ExecuteScalarAsync() as string;

        if (status == null) return null;
        if (status == "Completed") return false;

        var deleteQuery = "DELETE FROM Appointments WHERE IdAppointment = @Id";
        using var deleteCmd = new SqlCommand(deleteQuery, connection);
        deleteCmd.Parameters.AddWithValue("@Id", id);
        await deleteCmd.ExecuteNonQueryAsync();
        return true;
    }
}