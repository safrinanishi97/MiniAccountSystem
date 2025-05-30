namespace MiniAccountSystem.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
