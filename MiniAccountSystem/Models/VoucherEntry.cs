using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAccountSystem.Models
{
    public class VoucherEntry
    {
        public int AccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}
