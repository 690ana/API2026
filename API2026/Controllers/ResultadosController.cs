using API2026.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelos.DTO;
using Servicios.Interfaces;

namespace API2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultadosController : ControllerBase
    {
        private readonly IResultados _resultados;

        public ResultadosController(IResultados resultados)
        {
            _resultados = resultados;
        }

        [HttpPost("Consolidar")]
        [Authorize(Roles = "Docente")]
        public IActionResult Consolidar([FromBody] List<CalificacionDTO>? calificaciones)
        {
            return this.ARespuesta(_resultados.Consolidar(calificaciones));
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        public IActionResult ListarConsolidados()
        {
            return Ok(_resultados.ListarConsolidados());
        }

        [HttpGet("Consultar")]
        [Authorize(Roles = "Estudiante")]
        public IActionResult Consultar()
        {
            var idEstudiante = User.FindFirst("IdEstudiante")?.Value;
            return this.ARespuesta(_resultados.ConsultarPorEstudiante(idEstudiante));
        }
    }
}
