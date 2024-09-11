namespace Arkad.Shared.Models
{
    public class HistorialUsuario
    {
        public string Id { get; set; }
        public string Glosa { get; set; }
        public string Descripcion { get; set; }
        public DateTime FCreado { get; set; }
        public bool BActivo { get; set; }
        public string UsuarioActualId { get; set; }
        public string UsuarioId { get; set; }
        public string UsuarioName { get; set; }
    }
}
