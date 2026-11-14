namespace Modelos.DTO
{
    public class TokenDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
