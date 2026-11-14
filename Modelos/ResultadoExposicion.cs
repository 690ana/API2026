namespace Modelos
{
    public class ResultadoExposicion
    {
        public string IdEstudiante { get; set; } = string.Empty;
        public int IdExposicion { get; set; }
        public string NombreExposicion { get; set; } = string.Empty;
        public double Promedio { get; set; }
        public int CantidadEvaluaciones { get; set; }
    }
}
