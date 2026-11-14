namespace Modelos
{
    public class CalificacionExposicion
    {
        public int Id { get; set; }
        public string IdEstudiante { get; set; } = string.Empty;
        public int IdExposicion { get; set; }
        public string NombreExposicion { get; set; } = string.Empty;
        public double Nota { get; set; }
    }
}
