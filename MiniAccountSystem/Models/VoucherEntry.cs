using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAccountSystem.Models
{
    public class VoucherEntry
    {
        public int Id { get; set; }

        // Foreign Key to Voucher
        public int VoucherId { get; set; }
        // [ForeignKey("VoucherId")] // If you use EF Core, you might add this
        // public Voucher Voucher { get; set; } // Navigation property

        public int AccountId { get; set; }
        // [ForeignKey("AccountId")] // If you use EF Core, you might add this
        // public Account Account { get; set; } // Assuming you have an Account model

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditAmount { get; set; }
    }
}
