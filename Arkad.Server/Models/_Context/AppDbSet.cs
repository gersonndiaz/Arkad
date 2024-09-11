using Microsoft.EntityFrameworkCore;

namespace Arkad.Server.Models._Context
{
    public class AppDbSet : DbContext
    {
        public virtual DbSet<Accidente> Accidentes { get; set; }
        public virtual DbSet<IndicadorPeriodo> IndicadorPeriodos { get; set; }
        public virtual DbSet<HistorialItem> HistorialItems { get; set; }
        public virtual DbSet<HistorialPeriodo> HistorialPeriodos { get; set; }
        public virtual DbSet<HistorialPeriodoAccidentes> HistorialPeriodoAccidentes { get; set; }
        public virtual DbSet<HistorialReparticion> HistorialReparticion { get; set; }
        public virtual DbSet<HistorialUsuario> HistorialUsuario { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Periodo> Periodos { get; set; }
        public virtual DbSet<Reparticion> Reparticiones { get; set; }
        public virtual DbSet<Rol> Roles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
    }
}
