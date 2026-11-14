using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Datos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Modelos;
using Modelos.DTO;
using Modelos.Resultados;
using Servicios.Interfaces;

namespace Servicios.Servicios
{
    public class ServicioAutenticacion : IAutenticacion
    {
        private readonly Api2026DbContext _contexto;
        private readonly IConfiguration _configuracion;

        public ServicioAutenticacion(Api2026DbContext contexto, IConfiguration configuracion)
        {
            _contexto = contexto;
            _configuracion = configuracion;
        }

        public Resultado<TokenDTO> Login(LoginDTO? datos)
        {
            if (datos is null || string.IsNullOrWhiteSpace(datos.Usuario) || string.IsNullOrWhiteSpace(datos.Clave))
                return Resultado<TokenDTO>.Falla(TipoResultado.SolicitudInvalida,
                    "Escribe el usuario y la clave.");

            if (EsDocenteDemo(datos))
            {
                var claimsDocente = new List<Claim>
                {
                    new(ClaimTypes.Name, datos.Usuario),
                    new(ClaimTypes.GivenName, "Docente"),
                    new(ClaimTypes.Role, "Docente")
                };

                return Resultado<TokenDTO>.Exito(CrearToken(claimsDocente, "Docente", "Docente"));
            }

            var estudiante = ValidarEstudiante(datos.Usuario, datos.Clave);
            if (estudiante is null)
                return Resultado<TokenDTO>.Falla(TipoResultado.NoAutorizado,
                    "Usuario o clave incorrectos.");

            // El token del estudiante lleva su código para que el servidor sepa
            // qué resultados consultar sin confiar en nada que envíe el cliente.
            var claimsEstudiante = new List<Claim>
            {
                new(ClaimTypes.Name, estudiante.Usuario),
                new(ClaimTypes.GivenName, estudiante.Nombre),
                new(ClaimTypes.Role, "Estudiante"),
                new("IdEstudiante", estudiante.IdEstudiante)
            };

            return Resultado<TokenDTO>.Exito(
                CrearToken(claimsEstudiante, estudiante.Nombre, "Estudiante"));
        }

        private bool EsDocenteDemo(LoginDTO datos) =>
            datos.Usuario == _configuracion["UsuarioDemo:Usuario"] &&
            datos.Clave == _configuracion["UsuarioDemo:Clave"];

        private UsuarioEstudiante? ValidarEstudiante(string usuario, string clave)
        {
            var usuarioNormalizado = usuario.Trim().ToLower();
            var estudiante = _contexto.Usuarios
                .FirstOrDefault(cuenta => cuenta.Usuario.ToLower() == usuarioNormalizado);

            if (estudiante is null)
                return null;

            var sal = Convert.FromBase64String(estudiante.Sal);
            var hashRecibido = Rfc2898DeriveBytes.Pbkdf2(clave, sal, 100000, HashAlgorithmName.SHA256, 32);
            var hashGuardado = Convert.FromBase64String(estudiante.ClaveHash);

            return CryptographicOperations.FixedTimeEquals(hashRecibido, hashGuardado) ? estudiante : null;
        }

        private TokenDTO CrearToken(IEnumerable<Claim> claims, string nombre, string rol)
        {
            var expira = DateTime.UtcNow.AddHours(1);
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Clave"]!));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuracion["Jwt:Emisor"],
                audience: _configuracion["Jwt:Audiencia"],
                claims: claims,
                expires: expira,
                signingCredentials: credenciales);

            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expira = expira,
                Rol = rol,
                Nombre = nombre
            };
        }
    }
}
