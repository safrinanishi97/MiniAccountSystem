// MiniAccountSystem.Pages.Vouchers.CreateModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MiniAccountSystem.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public VoucherDto Voucher { get; set; } = new();

        public List<SelectListItem> AccountList { get; set; } = new();

        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(IConfiguration config, UserManager<IdentityUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            LoadAccounts();
            // One default line
            Voucher.VoucherDetails.Add(new VoucherDetailDto());
            Voucher.CreatedDate = DateTime.Now;
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            ViewData["UserWithRole"] = $"{User.Identity?.Name} ({roles.FirstOrDefault()})";
        }

        public async Task <IActionResult> OnPost()
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
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "User";

                // Format: "Username (Role)"
                var createdBy = $"{User.Identity?.Name ?? "System"} ({primaryRole})";
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
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

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
    }
}