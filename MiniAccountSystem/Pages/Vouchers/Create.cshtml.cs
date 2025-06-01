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
                // Log the error here
                // For now, fall back to empty list
                AccountList = new List<SelectListItem>();
            }
        }
    }

}


//private void LoadAccounts()
//{
//    string connectionString = _config.GetConnectionString("DefaultConnection");

//    using (var con = new SqlConnection(connectionString))
//    {
//        var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts ORDER BY Name", con);
//        con.Open();
//        var reader = cmd.ExecuteReader();

//        AccountList = new List<SelectListItem>();
//        while (reader.Read())
//        {
//            AccountList.Add(new SelectListItem
//            {
//                Text = reader["Name"].ToString(),
//                Value = reader["Id"].ToString()
//            });
//        }
//    }
//}







//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Data.SqlClient;
//using MiniAccountSystem.Models;
//using System.Data;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.Linq; // For .Sum() method

//namespace MiniAccountSystem.Pages.Vouchers
//{
//    public class CreateModel : PageModel
//    {
//        private readonly IConfiguration _configuration;

//        // Constructor to inject IConfiguration
//        public CreateModel(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        // Properties bound from the form
//        [BindProperty]
//        public string VoucherType { get; set; } = "Journal"; // Default to Journal

//        [BindProperty]
//        public string ReferenceNo { get; set; }

//        [BindProperty]
//        public DateTime VoucherDate { get; set; } = DateTime.Today;

//        [BindProperty]
//        public List<VoucherEntry> Entries { get; set; } = new List<VoucherEntry>
//        {
//            new VoucherEntry(), // First default row
//            new VoucherEntry()  // Second default row
//        };

//        // Property to hold the list of accounts for the dropdown
//        public SelectList AccountList { get; set; }

//        // Message property for displaying success/error messages on the page
//        [TempData] // TempData ensures the message persists for one redirect
//        public string Message { get; set; }
//        [TempData]
//        public string ErrorMessage { get; set; }

//        // Handles GET requests to populate the form (e.g., when the page loads)
//        public async Task OnGetAsync()
//        {
//            // Initialize Entries with default rows if it's null (e.g., on first load)
//            if (Entries == null || !Entries.Any())
//            {
//                Entries = new List<VoucherEntry>
//                {
//                    new VoucherEntry(),
//                    new VoucherEntry()
//                };
//            }
//            // Load accounts for the dropdown
//            await LoadAccountsAsync();
//        }

//        // Helper method to load accounts from the database
//        private async Task LoadAccountsAsync()
//        {
//            var accounts = new List<ChartOfAccount>();
//            string connectionString = _configuration.GetConnectionString("DefaultConnection");

//            if (string.IsNullOrEmpty(connectionString))
//            {
//                ErrorMessage = "Database connection string 'DefaultConnection' is missing or empty.";
//                Console.WriteLine(ErrorMessage); // Log to console for debugging
//                AccountList = new SelectList(new List<ChartOfAccount>(), "Id", "Name"); // Provide empty list
//                return;
//            }

//            try
//            {
//                using (var connection = new SqlConnection(connectionString))
//                {
//                    var command = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts ORDER BY Name", connection); // Order by Name for better UX
//                    await connection.OpenAsync();
//                    using (var reader = await command.ExecuteReaderAsync())
//                    {
//                        while (await reader.ReadAsync())
//                        {
//                            accounts.Add(new ChartOfAccount
//                            {
//                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                                Name = reader.GetString(reader.GetOrdinal("Name"))
//                            });
//                        }
//                    }
//                }
//            }
//            catch (SqlException ex)
//            {
//                ErrorMessage = $"Database error loading accounts: {ex.Message}";
//                Console.WriteLine($"SQL Error fetching accounts: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                ErrorMessage = $"An unexpected error occurred while loading accounts: {ex.Message}";
//                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
//            }
//            AccountList = new SelectList(accounts, "Id", "Name");
//        }

//        // Handles POST requests when the form is submitted
//        public async Task<IActionResult> OnPostAsync()
//        {
//            // Reload accounts in case of validation failure to re-populate dropdowns
//            await LoadAccountsAsync();

//            // Check server-side model validation (e.g., [Required] attributes)
//            if (!ModelState.IsValid)
//            {
//                ErrorMessage = "Please correct the form errors.";
//                return Page();
//            }

//            // Custom server-side validation for entries count
//            if (Entries == null || Entries.Count < 2)
//            {
//                ModelState.AddModelError("", "At least two entries (one debit, one credit) are required for a valid voucher.");
//                ErrorMessage = "At least two entries (one debit, one credit) are required for a valid voucher.";
//                return Page();
//            }

//            // Calculate total debit and total credit from submitted entries
//            decimal totalDebit = Entries.Sum(e => e.DebitAmount);
//            decimal totalCredit = Entries.Sum(e => e.CreditAmount);

//            // Custom server-side validation for debit/credit balance
//            if (totalDebit != totalCredit)
//            {
//                ModelState.AddModelError("", "Total debits must equal total credits.");
//                ErrorMessage = "Total debits must equal total credits.";
//                return Page();
//            }

//            // Validate that each entry has either a debit or a credit, but not both
//            foreach (var entry in Entries)
//            {
//                if (entry.DebitAmount > 0 && entry.CreditAmount > 0)
//                {
//                    ModelState.AddModelError("", "An entry cannot have both debit and credit amounts.");
//                    ErrorMessage = "An entry cannot have both debit and credit amounts.";
//                    return Page();
//                }
//            }

//            // Prepare DataTable for SQL Server Table-Valued Parameter (TVP)
//            DataTable entryTable = new DataTable();
//            entryTable.Columns.Add("AccountId", typeof(int));
//            entryTable.Columns.Add("DebitAmount", typeof(decimal));
//            entryTable.Columns.Add("CreditAmount", typeof(decimal));

//            foreach (var entry in Entries)
//            {
//                // Only add entries that have either a debit or a credit amount
//                if (entry.DebitAmount > 0 || entry.CreditAmount > 0)
//                {
//                    entryTable.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);
//                }
//            }

//            // Ensure there are actual entries to save after filtering empty ones
//            if (entryTable.Rows.Count < 2)
//            {
//                ModelState.AddModelError("", "Please ensure at least two non-zero entries are provided.");
//                ErrorMessage = "Please ensure at least two non-zero entries are provided.";
//                return Page();
//            }

//            string connectionString = _configuration.GetConnectionString("DefaultConnection");
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                ErrorMessage = "Database connection string 'DefaultConnection' is missing or empty.";
//                Console.WriteLine(ErrorMessage);
//                return Page();
//            }

//            try
//            {
//                using (SqlConnection connection = new SqlConnection(connectionString))
//                {
//                    using (SqlCommand command = new SqlCommand("sp_SaveVoucher", connection))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;

//                        // Add parameters for the main Voucher details
//                        command.Parameters.AddWithValue("@VoucherType", VoucherType);
//                        command.Parameters.AddWithValue("@ReferenceNo", ReferenceNo);
//                        command.Parameters.AddWithValue("@VoucherDate", VoucherDate);
//                        // CreatedBy will be handled by ASP.NET Identity later, or set to NULL/default for now
//                        // For now, let's pass a placeholder or null if not yet implemented
//                        command.Parameters.AddWithValue("@CreatedBy", User.Identity.IsAuthenticated ? User.Identity.Name : (object)DBNull.Value);


//                        // Add the Table-Valued Parameter for entries
//                        var tvpParam = command.Parameters.AddWithValue("@Entries", entryTable);
//                        tvpParam.SqlDbType = SqlDbType.Structured;
//                        tvpParam.TypeName = "VoucherEntryType"; // This must match your SQL Server User-Defined Table Type

//                        await connection.OpenAsync();
//                        await command.ExecuteNonQueryAsync();
//                        connection.Close();

//                        Message = "Voucher saved successfully!";
//                        return RedirectToPage("/Vouchers/Index"); // Redirect to the list page on success
//                    }
//                }
//            }
//            catch (SqlException ex)
//            {
//                ErrorMessage = $"Database Error: {ex.Message}";
//                Console.WriteLine($"SQL Error saving voucher: {ex.Message}");
//                return Page();
//            }
//            catch (Exception ex)
//            {
//                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
//                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
//                return Page();
//            }
//        }
//    }
//}