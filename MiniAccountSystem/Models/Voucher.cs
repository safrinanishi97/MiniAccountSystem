using System.ComponentModel.DataAnnotations;

namespace MiniAccountSystem.Models
{

    public class VoucherDetailDto
    {
        public int AccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class VoucherDto
    {
        public int? VoucherId { get; set; }  // null when creating
        public string VoucherType { get; set; } // Journal, Payment, Receipt
        public DateTime VoucherDate { get; set; }
        public string ReferenceNo { get; set; }

        public List<VoucherDetailDto> VoucherDetails { get; set; } = new();
    }



    public class VoucherDetailListDto
    {
        public int VoucherDetailId { get; set; }
        public int AccountId { get; set; }
        public string? AccountName { get; set; }  // From joined Accounts table
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class VoucherListDto
    {
        public int VoucherId { get; set; }
        public string? VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? ReferenceNo { get; set; }

        public List<VoucherDetailListDto> VoucherDetails { get; set; } = new();
    }


    //public class Voucher
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    [StringLength(50)]
    //    public string VoucherType { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    public string ReferenceNo { get; set; }

    //    [Required]
    //    [DataType(DataType.Date)]
    //    public DateTime VoucherDate { get; set; }

    //    [Required]
    //    public decimal TotalDebit { get; set; }

    //    [Required]
    //    public decimal TotalCredit { get; set; }

    //    [DataType(DataType.DateTime)]
    //    public DateTime? CreatedDate { get; set; } // Nullable, as it has a default value in DB

    //    [StringLength(256)]
    //    public string CreatedBy { get; set; }

    //    // Optional: To hold associated entries if you want to display them with the voucher
    //    public List<VoucherEntry> Entries { get; set; } = new List<VoucherEntry>();
    //}
}
