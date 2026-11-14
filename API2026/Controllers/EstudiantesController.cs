using API2026.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelos.DTO;
using Servicios.Interfaces;


namespace API2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Docente")]
    public class EstudiantesController : ControllerBase
    {
        private readonly IEstudiantes _estudiantes;

        public EstudiantesController(IEstudiantes estudiantes)
        {
            _estudiantes = estudiantes;
        }

        [HttpPost("Registrar")]
        public IActionResult Registrar([FromBody] RegistroEstudianteDTO estudiante)
        {
            return this.ARespuesta(_estudiantes.Registrar(estudiante));
        }
    }
}
