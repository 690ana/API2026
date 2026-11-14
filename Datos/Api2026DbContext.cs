using Microsoft.EntityFrameworkCore;
using Modelos;

namespace Datos
{
    public class Api2026DbContext : DbContext
    {
        public Api2026DbContext(DbContextOptions<Api2026DbContext> options)
            : base(options)
        {
        }

        public DbSet<UsuarioEstudiante> Usuarios => Set<UsuarioEstudiante>();
        public DbSet<CalificacionExposicion> Calificaciones => Set<CalificacionExposicion>();
        public DbSet<ResultadoExposicion> Resultados => Set<ResultadoExposicion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // El código del estudiante identifica la cuenta y el usuario no se repite.
            modelBuilder.Entity<UsuarioEstudiante>(entidad =>
            {
                entidad.HasKey(estudiante => estudiante.IdEstudiante);
                entidad.HasIndex(estudiante => estudiante.Usuario).IsUnique();
            });

            // Solo puede existir un resultado por estudiante y exposición.
            modelBuilder.Entity<ResultadoExposicion>()
                .HasKey(resultado => new { resultado.IdEstudiante, resultado.IdExposicion });
        }
    }
}
