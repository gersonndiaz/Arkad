using System.ComponentModel.DataAnnotations.Schema;

namespace Arkad.Shared.Models
{
    public class Item
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Position { get; set; }

        public string Type { get; set; }

        public string Formula { get; set; }

        public string FormulaAux { get; set; }

        public string FormulaTest { get; set; }

        public bool Auto { get; set; }

        public bool Monthly { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string GroupId { get; set; }

        #region VIRTUAL
        public Group Group { get; set; }
        #endregion VIRTUAL
    }

    public enum TypeEnum
    {
        VALUE = 1,
        AVERAGE = 2,
        FORMULA = 3
    }

    public static class TipoEnumExtensions
    {
        public static string ToStringValue(this TypeEnum type)
        {
            return type switch
            {
                TypeEnum.VALUE => "VALUE",
                TypeEnum.AVERAGE => "AVERAGE",
                TypeEnum.FORMULA => "FORMULA",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
