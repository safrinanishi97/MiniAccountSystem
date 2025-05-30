namespace MiniAccountSystem.Models
{
    public class Voucher
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherType { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceNo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; }

        [Required]
        public decimal TotalDebit { get; set; }

        [Required]
        public decimal TotalCredit { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; } // Nullable, as it has a default value in DB

        [StringLength(256)]
        public string CreatedBy { get; set; }

        // Optional: To hold associated entries if you want to display them with the voucher
        public List<VoucherEntry> Entries { get; set; } = new List<VoucherEntry>();
    }
}
