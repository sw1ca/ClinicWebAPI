using Microsoft.AspNetCore.Mvc;

namespace ClinicWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var appointments = _service.GetAllAppointments();
            return Ok(appointments);
        }
    }
}
