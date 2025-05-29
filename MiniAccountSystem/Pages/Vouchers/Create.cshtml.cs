using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public CreateModel(IConfiguration configuration) => _configuration = configuration;

        [BindProperty]
        public string VoucherType { get; set; }

        [BindProperty]
        public string ReferenceNo { get; set; }

        [BindProperty]
        public DateTime VoucherDate { get; set; }

        [BindProperty]
        public List<VoucherEntry> Entries { get; set; } = new()
    {
        new VoucherEntry(), new VoucherEntry() // 2 default rows
    };

        public List<SelectListItem> AccountList { get; set; }

        public async Task OnGetAsync()
        {
            AccountList = new List<SelectListItem>();
            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new("SELECT Id, Name FROM ChartOfAccounts", con);

            await con.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                AccountList.Add(new SelectListItem(reader["Name"].ToString(), reader["Id"].ToString()));
            }
            con.Close();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            DataTable entryTable = new();
            entryTable.Columns.Add("AccountId", typeof(int));
            entryTable.Columns.Add("DebitAmount", typeof(decimal));
            entryTable.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var entry in Entries)
            {
                entryTable.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);
            }

            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new("sp_SaveVoucher", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@VoucherType", VoucherType);
            cmd.Parameters.AddWithValue("@ReferenceNo", ReferenceNo);
            cmd.Parameters.AddWithValue("@VoucherDate", VoucherDate);

            var tvpParam = cmd.Parameters.AddWithValue("@Entries", entryTable);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "VoucherEntryType";

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            con.Close();

            return RedirectToPage("/Vouchers/List");
        }

    }
}
