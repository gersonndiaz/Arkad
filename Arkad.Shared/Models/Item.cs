namespace Arkad.Shared.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool BManual { get; set; } // Mapeo a un entero (0 o 1)
        public int Posicion { get; set; }
        public string Tipo { get; set; }
        public string Formula { get; set; }
        public string FormulaAux { get; set; }
        public string FormulaTest { get; set; }
        public DateTime FCreado { get; set; } // Convertido a DateTime para facilidad en C#
        public bool? BMandante { get; set; } // Mapeo a un entero (0 o 1)
        public bool? BContratista { get; set; } // Mapeo a un entero (0 o 1)
        public bool BActivo { get; set; } // Mapeo a un entero (0 o 1)
    }

    public enum TipoEnum
    {
        DATO = 1,
        PROMEDIO = 2,
        FORMULA = 3
    }
    public static class TipoEnumExtensions
    {
        public static string ToStringValue(this TipoEnum tipo)
        {
            return tipo switch
            {
                TipoEnum.DATO => "DATO",
                TipoEnum.PROMEDIO => "PROMEDIO",
                TipoEnum.FORMULA => "FORMULA",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
