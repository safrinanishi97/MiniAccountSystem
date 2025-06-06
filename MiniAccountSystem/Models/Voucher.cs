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
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }  
        public DateTime? UpdatedDate { get; set; }  
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

    //public class VoucherListDto
    //{
    //    public int VoucherId { get; set; }
    //    public string? VoucherType { get; set; }
    //    public DateTime VoucherDate { get; set; }
    //    public string? ReferenceNo { get; set; }
    //    public string CreatedBy { get; set; }
    //    public DateTime CreatedDate { get; set; }


    //    public List<VoucherDetailListDto> VoucherDetails { get; set; } = new();
    //}

    public class VoucherListDto
    {
        public int VoucherId { get; set; }
        public string VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }       
        public DateTime? UpdatedDate { get; set; }  

        public List<VoucherDetailListDto> VoucherDetails { get; set; } = new List<VoucherDetailListDto>();
    }



}
