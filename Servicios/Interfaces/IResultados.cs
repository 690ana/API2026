using Modelos;
using Modelos.DTO;
using Modelos.Resultados;

namespace Servicios.Interfaces
{
    public interface IResultados
    {
        Resultado<List<ResultadoExposicion>> Consolidar(List<CalificacionDTO>? calificaciones);
        List<ResultadoExposicion> ListarConsolidados();
        Resultado<List<ResultadoExposicion>> ConsultarPorEstudiante(string? idEstudiante);
    }
}
