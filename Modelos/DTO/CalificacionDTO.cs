namespace Modelos.DTO
{
    // Nota que el docente envía para consolidar. Es la entrada del API;
    // la entidad CalificacionExposicion es lo que se persiste.
    public class CalificacionDTO
    {
        public string IdEstudiante { get; set; } = string.Empty;
        public int IdExposicion { get; set; }
        public string NombreExposicion { get; set; } = string.Empty;
        public double Nota { get; set; }
    }
}
