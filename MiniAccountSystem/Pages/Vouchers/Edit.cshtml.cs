using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.Vouchers
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public VoucherDto Voucher { get; set; } = new();

        public List<SelectListItem> AccountList { get; set; } = new();

        private readonly IConfiguration _config;
        public EditModel(IConfiguration config)
        {
            _config = config;
        }

        //public void OnGet(int id)
        //{
        //    LoadAccounts();

        //    using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        //    using var cmd = new SqlCommand("sp_GetVoucherById", conn)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };
        //    cmd.Parameters.AddWithValue("@VoucherId", id);
        //    conn.Open();
        //    using var reader = cmd.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        Voucher.VoucherId = id;
        //        Voucher.VoucherType = reader["VoucherType"]?.ToString() ?? "";
        //        Voucher.VoucherDate = Convert.ToDateTime(reader["VoucherDate"]);
        //        Voucher.ReferenceNo = reader["ReferenceNo"]?.ToString() ?? "";
        //    }

        //    // Next Result: Voucher Details
        //    if (reader.NextResult())
        //    {
        //        while (reader.Read())
        //        {
        //            Voucher.VoucherDetails.Add(new VoucherDetailDto
        //            {
        //                AccountId = Convert.ToInt32(reader["AccountId"]),
        //                DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
        //                CreditAmount = Convert.ToDecimal(reader["CreditAmount"])
        //            });
        //        }
        //    }
        //}


        public void OnGet(int id)
        {
            LoadAccounts();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetVoucherById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@VoucherId", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                Voucher.VoucherId = id;
                Voucher.VoucherType = reader["VoucherType"]?.ToString() ?? "";
                Voucher.VoucherDate = Convert.ToDateTime(reader["VoucherDate"]);
                Voucher.ReferenceNo = reader["ReferenceNo"]?.ToString() ?? "";
            }

            // Next Result: Voucher Details
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    Voucher.VoucherDetails.Add(new VoucherDetailDto
                    {
                        AccountId = Convert.ToInt32(reader["AccountId"]),
                        DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                        CreditAmount = Convert.ToDecimal(reader["CreditAmount"])
                    });
                }
            }
        }

        public IActionResult OnPost()
        {
            LoadAccounts();

            decimal debitSum = Voucher.VoucherDetails.Sum(d => d.DebitAmount);
            decimal creditSum = Voucher.VoucherDetails.Sum(c => c.CreditAmount);

            if (debitSum != creditSum)
            {
                ViewData["Error"] = "Total Debit must equal Total Credit!";
                return Page();
            }

            try
            {
                using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_SaveVoucher", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Action", "Update");
                cmd.Parameters.AddWithValue("@VoucherId", Voucher.VoucherId);
                cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
                cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
                cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);
                cmd.Parameters.AddWithValue("@CreatedBy", User.Identity?.Name ?? "Admin");

                // Table-valued parameter
                var dt = new DataTable();
                dt.Columns.Add("AccountId", typeof(int));
                dt.Columns.Add("DebitAmount", typeof(decimal));
                dt.Columns.Add("CreditAmount", typeof(decimal));

                foreach (var item in Voucher.VoucherDetails)
                    dt.Rows.Add(item.AccountId, item.DebitAmount, item.CreditAmount);

                var param = cmd.Parameters.AddWithValue("@VoucherDetails", dt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.VoucherDetailType";

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return Page();
            }
        }

        private void LoadAccounts()
        {
            AccountList = new List<SelectListItem>
        {
            new("Cash", "1"),
            new("Bank", "2"),
            new("Receivable", "3")
        };
        }
    }

}
