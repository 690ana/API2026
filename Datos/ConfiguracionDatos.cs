using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Datos
{
    // Único lugar que conoce cómo se arma la capa de Datos.
    // El proyecto API solo llama a estos métodos; no manipula el DbContext.
    public static class ConfiguracionDatos
    {
        public static IServiceCollection AgregarDatos(this IServiceCollection services)
        {
            services.AddDbContext<Api2026DbContext>(opciones =>
                opciones.UseInMemoryDatabase("Api2026Pruebas"));
            return services;
        }

        public static void InicializarDatos(this IServiceProvider provider)
        {
            using var alcance = provider.CreateScope();
            var contexto = alcance.ServiceProvider.GetRequiredService<Api2026DbContext>();
            contexto.Database.EnsureCreated();
        }
    }
}
