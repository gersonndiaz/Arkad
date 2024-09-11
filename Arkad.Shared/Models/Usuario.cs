namespace Arkad.Shared.Models
{
    public class Usuario
    {
        public string Id { get; set; }
        public string Run { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string ClaveNew { get; set; }
        public string ClaveRepeat { get; set; }
        public DateTime? FUltimaConexion { get; set; }
        public DateTime FCreado { get; set; }
        public DateTime FModificado { get; set; }
        public bool BActivo { get; set; }
        public string RolId { get; set; }
        public string RolName { get; set; }
    }
}
