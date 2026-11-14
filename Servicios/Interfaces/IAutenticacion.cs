using Modelos.DTO;
using Modelos.Resultados;

namespace Servicios.Interfaces
{
    public interface IAutenticacion
    {
      
        Resultado<TokenDTO> Login(LoginDTO? datos);
    }
}
