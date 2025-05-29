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
        public DateTime VoucherDate { get; set; } = DateTime.Today;


        [BindProperty]
        public List<VoucherEntry> Entries { get; set; } = new()
    {
        new VoucherEntry(), new VoucherEntry() // 2 default rows
    };

        public SelectList AccountList { get; set; }

        public async Task OnGetAsync()
        {
            Entries = new List<VoucherEntry>
            {
                new VoucherEntry(),
                new VoucherEntry()
            };

            AccountList = new SelectList(GetAccountsFromDB(), "Id", "Name");
        }

        private List<ChartOfAccount> GetAccountsFromDB()
        {
            var accounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing!");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new ChartOfAccount
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            return accounts;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Entries == null || Entries.Count < 2)
            {
                ModelState.AddModelError("", "At least two entries required");
                return Page();
            }

            decimal totalDebit = Entries.Sum(e => e.DebitAmount);
            decimal totalCredit = Entries.Sum(e => e.CreditAmount);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "Total debits must equal total credits");
                return Page();
            }

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
