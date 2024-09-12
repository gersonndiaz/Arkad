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

        public bool Auto { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public bool Active { get; set; }

        public string GroupId { get; set; }
    }
}
