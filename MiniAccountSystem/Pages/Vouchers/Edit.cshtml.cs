using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(IConfiguration config, UserManager<IdentityUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task OnGetAsync(int id)
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
                Voucher.CreatedBy = reader["CreatedBy"]?.ToString() ?? "";
                Voucher.CreatedDate = reader["CreatedDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["CreatedDate"])
                    : DateTime.Now;
                Voucher.UpdatedBy = reader["UpdatedBy"]?.ToString(); 
                Voucher.UpdatedDate = reader["UpdatedDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["UpdatedDate"])
                    : (DateTime?)null;
            }

            // Load details
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

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            ViewData["UserWithRole"] = $"{User.Identity?.Name} ({roles.FirstOrDefault()})";
        }

        public async Task<IActionResult> OnPostAsync()
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

                var createdBy = $"{User.Identity?.Name ?? "System"} ({primaryRole})";
                var updatedBy = $"{User.Identity?.Name ?? "System"} ({primaryRole})";
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
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
       

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
                using var con = new SqlConnection(connectionString);
                var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts ORDER BY Name", con);
                con.Open();
                var reader = cmd.ExecuteReader();

                AccountList = new List<SelectListItem>
                {
                    new("-- Select Account --", "")
                };
                while (reader.Read())
                {
                    AccountList.Add(new SelectListItem
                    {
                        Text = reader["Name"].ToString(),
                        Value = reader["Id"].ToString()
                    });
                }
            }
            catch
            {
                AccountList = new List<SelectListItem>();
            }
        }
    }
}
