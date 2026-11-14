using Microsoft.AspNetCore.Mvc;
using Modelos.Resultados;

namespace API2026.Extensions
{
    public static class ResultadoExtensions
    {
        public static IActionResult ARespuesta<T>(this ControllerBase controlador, Resultado<T> resultado)
        {
            if (resultado.Exitoso)
                return controlador.Ok(resultado.Valor);

            return resultado.Tipo switch
            {
                TipoResultado.SolicitudInvalida => controlador.BadRequest(resultado.Mensaje),
                TipoResultado.NoEncontrado => controlador.NotFound(resultado.Mensaje),
                TipoResultado.Conflicto => controlador.Conflict(resultado.Mensaje),
                TipoResultado.NoAutorizado => controlador.Unauthorized(resultado.Mensaje),
                _ => controlador.StatusCode(500, resultado.Mensaje)
            };
        }
    }
}
