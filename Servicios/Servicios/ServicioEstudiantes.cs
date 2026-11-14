using System.Security.Cryptography;
using Datos;
using Modelos;
using Modelos.DTO;
using Modelos.Resultados;
using Servicios.Interfaces;

namespace Servicios.Servicios
{
    public class ServicioEstudiantes : IEstudiantes
    {
        private readonly Api2026DbContext _contexto;

        public ServicioEstudiantes(Api2026DbContext contexto)
        {
            _contexto = contexto;
        }

        public Resultado<string> Registrar(RegistroEstudianteDTO? datos)
        {
            if (datos is null)
                return Resultado<string>.Falla(TipoResultado.SolicitudInvalida,
                    "Completa todos los datos del estudiante.");

            datos.IdEstudiante = (datos.IdEstudiante ?? string.Empty).Trim();
            datos.Nombre = (datos.Nombre ?? string.Empty).Trim();
            datos.Usuario = (datos.Usuario ?? string.Empty).Trim();
            datos.Clave = datos.Clave ?? string.Empty;

            if (datos.IdEstudiante == "" || datos.Nombre == "" || datos.Usuario == "" || datos.Clave == "")
                return Resultado<string>.Falla(TipoResultado.SolicitudInvalida,
                    "Completa todos los datos del estudiante.");

            if (datos.Clave.Length < 6)
                return Resultado<string>.Falla(TipoResultado.SolicitudInvalida,
                    "La clave debe tener al menos 6 caracteres.");

            var usuarioNormalizado = datos.Usuario.ToLower();
            var yaExiste = _contexto.Usuarios.Any(cuenta =>
                cuenta.IdEstudiante == datos.IdEstudiante ||
                cuenta.Usuario.ToLower() == usuarioNormalizado);

            if (yaExiste)
                return Resultado<string>.Falla(TipoResultado.Conflicto,
                    "Ya existe una cuenta con ese código o usuario.");

            // La clave nunca se guarda en texto plano: se guarda la sal y el hash.
            var sal = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(datos.Clave, sal, 100000, HashAlgorithmName.SHA256, 32);

            var estudiante = new UsuarioEstudiante
            {
                IdEstudiante = datos.IdEstudiante,
                Nombre = datos.Nombre,
                Usuario = datos.Usuario,
                Sal = Convert.ToBase64String(sal),
                ClaveHash = Convert.ToBase64String(hash)
            };

            _contexto.Usuarios.Add(estudiante);
            _contexto.SaveChanges();

            return Resultado<string>.Exito("Cuenta de estudiante creada.");
        }
    }
}
