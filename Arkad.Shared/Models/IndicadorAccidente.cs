namespace Arkad.Shared.Models
{
    public class IndicadorAccidente
    {
        public Periodo Periodo { get; set; }
        public Reparticion Reparticion { get; set; }
        public List<ItemAccidente> AccidentesMandante { get; set; }
        public List<ItemAccidente> AccidentesContratista { get; set; }
        public List<ItemAccidente> AccidentesGlobal { get; set; }
    }

    public class ItemAccidente
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool BManual { get; set; } // Mapeo a un entero (0 o 1)
        public int Posicion { get; set; }
        public string Tipo { get; set; }
        public string Formula { get; set; }
        public string FormulaAux { get; set; }
        public DateTime FCreado { get; set; } // Convertido a DateTime para facilidad en C#
        public bool? BMandante { get; set; } // Mapeo a un entero (0 o 1)
        public bool? BContratista { get; set; } // Mapeo a un entero (0 o 1)
        public bool BActivo { get; set; } // Mapeo a un entero (0 o 1)

        public Accidente Accidente { get; set; } = new Accidente();
    }

    public class Accidente
    {
        public string Id { get; set; }
        public float Valor { get; set; } = 0;
        public string Tipo { get; set; }
        public string ReparticionId { get; set; }
        public string PeriodoId { get; set; }
        public string ItemId { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FCreado { get; set; }
        public bool BActivo { get; set; }
    }
}
