using Datos;
using Modelos;
using Modelos.DTO;
using Modelos.Resultados;
using Servicios.Interfaces;

namespace Servicios.Servicios
{
    public class ServicioResultados : IResultados
    {
        private readonly Api2026DbContext _contexto;

        public ServicioResultados(Api2026DbContext contexto)
        {
            _contexto = contexto;
        }

        public Resultado<List<ResultadoExposicion>> Consolidar(List<CalificacionDTO>? calificaciones)
        {
            if (calificaciones is null || calificaciones.Count == 0)
                return Resultado<List<ResultadoExposicion>>.Falla(TipoResultado.SolicitudInvalida,
                    "Agrega al menos una calificación antes de consolidar.");

            if (calificaciones.Any(calificacion =>
                    string.IsNullOrWhiteSpace(calificacion.IdEstudiante) ||
                    calificacion.IdExposicion < 1 ||
                    string.IsNullOrWhiteSpace(calificacion.NombreExposicion) ||
                    calificacion.Nota < 0 || calificacion.Nota > 5))
            {
                return Resultado<List<ResultadoExposicion>>.Falla(TipoResultado.SolicitudInvalida,
                    "Verifica el código del estudiante, la exposición y las notas entre 0 y 5.");
            }

            var codigos = calificaciones
                .Select(calificacion => calificacion.IdEstudiante.Trim())
                .Distinct()
                .ToList();

            var codigosExistentes = _contexto.Usuarios
                .Where(cuenta => codigos.Contains(cuenta.IdEstudiante))
                .Select(cuenta => cuenta.IdEstudiante)
                .ToHashSet();

            var codigoFaltante = codigos.FirstOrDefault(codigo => !codigosExistentes.Contains(codigo));
            if (codigoFaltante is not null)
                return Resultado<List<ResultadoExposicion>>.Falla(TipoResultado.NoEncontrado,
                    $"No existe una cuenta para el estudiante {codigoFaltante}.");

            // Solo se reemplazan las calificaciones y los resultados de los pares
            // (estudiante, exposición) que llegan en esta consolidación;
            // el resto de la base de datos queda intacto.
            var pares = calificaciones
                .Select(calificacion => new
                {
                    IdEstudiante = calificacion.IdEstudiante.Trim(),
                    calificacion.IdExposicion
                })
                .Distinct()
                .ToList();

            foreach (var par in pares)
            {
                _contexto.Calificaciones.RemoveRange(_contexto.Calificaciones
                    .Where(calificacion =>
                        calificacion.IdEstudiante == par.IdEstudiante &&
                        calificacion.IdExposicion == par.IdExposicion));

                _contexto.Resultados.RemoveRange(_contexto.Resultados
                    .Where(resultado =>
                        resultado.IdEstudiante == par.IdEstudiante &&
                        resultado.IdExposicion == par.IdExposicion));
            }

            _contexto.Calificaciones.AddRange(calificaciones.Select(calificacion => new CalificacionExposicion
            {
                IdEstudiante = calificacion.IdEstudiante.Trim(),
                IdExposicion = calificacion.IdExposicion,
                NombreExposicion = calificacion.NombreExposicion.Trim(),
                Nota = calificacion.Nota
            }));

            var resultados = calificaciones
                .GroupBy(calificacion => new
                {
                    IdEstudiante = calificacion.IdEstudiante.Trim(),
                    calificacion.IdExposicion
                })
                .Select(grupo => new ResultadoExposicion
                {
                    IdEstudiante = grupo.Key.IdEstudiante,
                    IdExposicion = grupo.Key.IdExposicion,
                    NombreExposicion = grupo.First().NombreExposicion,
                    Promedio = grupo.Average(calificacion => calificacion.Nota),
                    CantidadEvaluaciones = grupo.Count()
                })
                .ToList();

            _contexto.Resultados.AddRange(resultados);
            _contexto.SaveChanges();

            return Resultado<List<ResultadoExposicion>>.Exito(resultados);
        }

        public List<ResultadoExposicion> ListarConsolidados()
        {
            return _contexto.Resultados
                .OrderBy(resultado => resultado.IdEstudiante)
                .ThenBy(resultado => resultado.IdExposicion)
                .ToList();
        }

        public Resultado<List<ResultadoExposicion>> ConsultarPorEstudiante(string? idEstudiante)
        {
            if (string.IsNullOrWhiteSpace(idEstudiante))
                return Resultado<List<ResultadoExposicion>>.Falla(TipoResultado.NoAutorizado,
                    "El token no contiene un estudiante válido.");

            var resultados = _contexto.Resultados
                .Where(resultado => resultado.IdEstudiante == idEstudiante)
                .OrderBy(resultado => resultado.IdExposicion)
                .ToList();

            return Resultado<List<ResultadoExposicion>>.Exito(resultados);
        }
    }
}
