using Modelos.DTO;
using Modelos.Resultados;

namespace Servicios.Interfaces
{
    public interface IEstudiantes
    {
        Resultado<string> Registrar(RegistroEstudianteDTO? datos);
    }
}
