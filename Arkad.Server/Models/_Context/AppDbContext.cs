using Microsoft.EntityFrameworkCore;

namespace Arkad.Server.Models._Context
{
    public class AppDbContext : AppDbSet
    {
        /// <summary>
        /// Configuración de la conexión
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            optionsBuilder
                .UseSqlite(configuration.GetConnectionString("ArkadConnection"))
                .UseLazyLoadingProxies();
        }
    }
}
