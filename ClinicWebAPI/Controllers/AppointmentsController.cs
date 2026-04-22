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
    }
}
