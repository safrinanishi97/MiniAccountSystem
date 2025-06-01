// MiniAccountSystem.Pages.Vouchers.CreateModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;
using System.Collections.Generic; // Make sure this is included
using System.Threading.Tasks;    // Make sure this is included

namespace MiniAccountSystem.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public VoucherDto Voucher { get; set; } = new();

        public List<SelectListItem> AccountList { get; set; } = new();

        private readonly IConfiguration _config;
        public CreateModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnGet()
        {
            LoadAccounts();
            // One default line
            Voucher.VoucherDetails.Add(new VoucherDetailDto());
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

                cmd.Parameters.AddWithValue("@Action", "Create");
                cmd.Parameters.AddWithValue("@VoucherId", DBNull.Value);
                cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
                cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
                cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);
                cmd.Parameters.AddWithValue("@CreatedBy", User.Identity?.Name ?? "Admin"); // Or fixed user

                // Build table-valued parameter
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

                return RedirectToPage("List");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return Page();
            }
        }


        private void LoadAccounts()
        {
            string connectionString = _config.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing!");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Connection string is missing!");
            }

            try
            {
                using (var con = new SqlConnection(connectionString))
                {

                    var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts ORDER BY Name", con);
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    AccountList = new List<SelectListItem>();

                    // Add default empty option (for better UX)
                    AccountList.Add(new SelectListItem("-- Select Account --", ""));
                    while (reader.Read())
                    {
                        AccountList.Add(new SelectListItem
                        {
                            Text = reader["Name"].ToString(),
                            Value = reader["Id"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AccountList = new List<SelectListItem>();
            }
        }

    //    private void LoadAccounts()
    //    {

    //        try
    //        {

    //            // Clear existing items first
    //            AccountList = new List<SelectListItem>();

    //            // Add default empty option (for better UX)
    //            AccountList.Add(new SelectListItem("-- Select Account --", ""));

    //            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    //            // Query to get active accounts sorted by name
    //            string query = @"SELECT c.Id, c.Name, 
    //            CASE WHEN c.ParentId IS NULL THEN c.Name 
    //            ELSE '    ' + c.Name END AS DisplayName
    //            FROM ChartOfAccounts c
              
    //            ORDER BY COALESCE(c.ParentId, c.Id), c.Id";

    //            using var command = new SqlCommand(query, connection);
    //            connection.Open();

    //            using var reader = command.ExecuteReader();

    //            while (reader.Read())
    //            {
    //                AccountList.Add(new SelectListItem
    //                {
    //                    Text = reader["Name"].ToString(),
    //                    Value = reader["Id"].ToString()
    //                });
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            // Log error (in production, use ILogger)
    //            Console.WriteLine($"Database error: {ex.Message}");

    //            // Show user-friendly message
    //            ViewData["ErrorMessage"] = "There is an unexpected error, Please try again!!";

    //            // Fallback to empty list
    //            AccountList = new List<SelectListItem>();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Unexpected error: {ex.Message}");
    //            ViewData["ErrorMessage"] = "An unexpected error";
    //            AccountList = new List<SelectListItem>();
    //        }
    //    }
    }

}


