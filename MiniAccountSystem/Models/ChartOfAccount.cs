namespace MiniAccountSystem.Models
{
    public class ChartOfAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }

        public List<ChartOfAccount> Children { get; set; } = new List<ChartOfAccount>();
    }

}
