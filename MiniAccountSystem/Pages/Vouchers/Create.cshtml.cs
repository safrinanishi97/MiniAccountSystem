//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Data.SqlClient;
//using MiniAccountSystem.Models;
//using System.Data;

//namespace MiniAccountSystem.Pages.Vouchers
//{
//    public class CreateModel : PageModel
//    {
//        private readonly IConfiguration _configuration;
//        public CreateModel(IConfiguration configuration) => _configuration = configuration;

//        [BindProperty]
//        public string VoucherType { get; set; }

//        [BindProperty]
//        public string ReferenceNo { get; set; }

//        [BindProperty]
//        public DateTime VoucherDate { get; set; } = DateTime.Today;


//        [BindProperty]
//        public List<VoucherEntry> Entries { get; set; } = new()
//    {
//        new VoucherEntry(), new VoucherEntry() // 2 default rows
//    };

//        public SelectList AccountList { get; set; }

//        public async Task OnGetAsync()
//        {
//            Entries = new List<VoucherEntry>
//            {
//                new VoucherEntry(),
//                new VoucherEntry()
//            };

//            // Await the asynchronous method call
//            AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name");
//        }

//        // Change return type to Task<List<ChartOfAccount>> and make it async
//        private async Task<List<ChartOfAccount>> GetAccountsFromDBAsync()
//        {
//            var accounts = new List<ChartOfAccount>();
//            string connectionString = _configuration.GetConnectionString("DefaultConnection")
//            ?? throw new ArgumentNullException("Connection string is missing!");

//            using (var connection = new SqlConnection(connectionString))
//            {
//                var command = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts", connection);
//                // Use async version of Open and ExecuteReader
//                await connection.OpenAsync();
//                using (var reader = await command.ExecuteReaderAsync())
//                {
//                    while (await reader.ReadAsync()) // Use async version of Read
//                    {
//                        accounts.Add(new ChartOfAccount
//                        {
//                            Id = (int)reader["Id"],
//                            Name = reader["Name"].ToString()
//                        });
//                    }
//                }
//            }

//            return accounts;
//        }
//        public async Task<IActionResult> OnPostAsync()
//        {
//            if (Entries == null || Entries.Count < 2)
//            {
//                ModelState.AddModelError("", "At least two entries required");
//                return Page();
//            }

//            decimal totalDebit = Entries.Sum(e => e.DebitAmount);
//            decimal totalCredit = Entries.Sum(e => e.CreditAmount);

//            if (totalDebit != totalCredit)
//            {
//                ModelState.AddModelError("", "Total debits must equal total credits");
//                return Page();
//            }

//            DataTable entryTable = new();
//            entryTable.Columns.Add("AccountId", typeof(int));
//            entryTable.Columns.Add("DebitAmount", typeof(decimal));
//            entryTable.Columns.Add("CreditAmount", typeof(decimal));

//            foreach (var entry in Entries)
//            {
//                entryTable.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);
//            }

//            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
//            using SqlCommand cmd = new("sp_SaveVoucher", con)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            cmd.Parameters.AddWithValue("@VoucherType", VoucherType);
//            cmd.Parameters.AddWithValue("@ReferenceNo", ReferenceNo);
//            cmd.Parameters.AddWithValue("@VoucherDate", VoucherDate);

//            var tvpParam = cmd.Parameters.AddWithValue("@Entries", entryTable);
//            tvpParam.SqlDbType = SqlDbType.Structured;
//            tvpParam.TypeName = "VoucherEntryType";

//            await con.OpenAsync();
//            await cmd.ExecuteNonQueryAsync();
//            con.Close();

//            return RedirectToPage("/Vouchers/List");
//        }

//    }
//}




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

        public SelectList AccountList { get; set; } // This will be initialized in OnGetAsync

        public async Task OnGetAsync()
        {
            // Always initialize Entries
            Entries = new List<VoucherEntry>
            {
                new VoucherEntry(),
                new VoucherEntry()
            };

            // Await the asynchronous database call
            AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name");
        }

        // Changed to async and returns Task<List<ChartOfAccount>>
        private async Task<List<ChartOfAccount>> GetAccountsFromDBAsync()
        {
            var accounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Add robust error handling for connection string
            if (string.IsNullOrEmpty(connectionString))
            {
                // Log the error (e.g., using ILogger) instead of throwing to allow page to load
                Console.WriteLine("Error: Connection string 'DefaultConnection' is missing or empty.");
                return accounts; // Return an empty list to prevent NullReferenceException
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts", connection);
                    await connection.OpenAsync(); // Use async open
                    using (var reader = await command.ExecuteReaderAsync()) // Use async reader
                    {
                        while (await reader.ReadAsync()) // Use async read
                        {
                            accounts.Add(new ChartOfAccount
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log the SQL exception (e.g., using ILogger)
                Console.WriteLine($"SQL Error fetching accounts: {ex.Message}");
                // You might want to add a Message property to your model to display this error on the page
                // Message = "Error loading accounts. Please try again later.";
            }
            catch (Exception ex)
            {
                // Log any other general exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            return accounts; // Ensure a list is always returned, even if empty due to errors.
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // You might also need to re-fetch AccountList here if validation fails
            // to re-render the dropdowns correctly on postback.
            if (!ModelState.IsValid)
            {
                AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name");
                return Page();
            }

            if (Entries == null || Entries.Count < 2)
            {
                ModelState.AddModelError("", "At least two entries required");
                AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name"); // Re-initialize
                return Page();
            }

            decimal totalDebit = Entries.Sum(e => e.DebitAmount);
            decimal totalCredit = Entries.Sum(e => e.CreditAmount);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "Total debits must equal total credits");
                AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name"); // Re-initialize
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

            try
            {
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                con.Close();
                return RedirectToPage("/Vouchers/List");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
                // Log the exception
                AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name"); // Re-initialize
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                // Log the exception
                AccountList = new SelectList(await GetAccountsFromDBAsync(), "Id", "Name"); // Re-initialize
                return Page();
            }
        }
    }
}