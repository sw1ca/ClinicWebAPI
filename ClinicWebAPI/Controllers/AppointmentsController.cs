using ClinicWebAPI.DTOs;
using ClinicWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentsService;

        public AppointmentsController(IAppointmentsService appointmentsService)
        {
            _appointmentsService = appointmentsService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? patientLastName)
        {
            var result = await _appointmentsService.GetAllAppointmentsAsync(status, patientLastName);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _appointmentsService.DeleteAppointmentAsync(id);
            if (result == null) return NotFound();
            if (result == false) return Conflict("Cannot delete completed appointment.");
            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _appointmentsService.GetAppointmentDetailsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            var result = await _appointmentsService.UpdateAppointmentAsync(id, dto);
            if (result == null) return NotFound();
            if (result == false) return Conflict("Cannot update completed appointment.");
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            try
            {
                var id = await _appointmentsService.CreateAppointmentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = id }, dto);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
