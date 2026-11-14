using API2026.Extensions;
using Microsoft.AspNetCore.Mvc;
using Modelos.DTO;
using Servicios.Interfaces;

namespace API2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAutenticacion _autenticacion;

        public AuthController(IAutenticacion autenticacion)
        {
            _autenticacion = autenticacion;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDTO? datos)
        {
            return this.ARespuesta(_autenticacion.Login(datos));
        }
    }
}
