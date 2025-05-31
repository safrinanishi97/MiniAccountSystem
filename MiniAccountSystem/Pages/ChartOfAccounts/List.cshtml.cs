using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using System.Drawing;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    public class ListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ChartOfAccount> Accounts { get; set; } = new List<ChartOfAccount>();
        public List<ChartOfAccount> FlatAccounts { get; set; } = new List<ChartOfAccount>(); // For table view

        public void OnGet()
        {
            LoadAccounts();
            BuildAccountTree();
        }

        //}

        public IActionResult OnPostDelete(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");

            try
            {
                using var con = new SqlConnection(connectionString);

                // First check if this account has any children
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM ChartOfAccounts WHERE ParentId = @AccountId", con);
                checkCmd.Parameters.AddWithValue("@AccountId", id);

                con.Open();
                var childCount = (int)checkCmd.ExecuteScalar();

                if (childCount > 0)
                {
                    TempData["ErrorMessage"] = "Cannot delete account because it has child accounts. Please delete or reassign the child accounts first.";
                    return RedirectToPage("List");
                }

                // If no children, proceed with deletion
                var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@AccountId", id);

                cmd.ExecuteNonQuery();
                TempData["SuccessMessage"] = "Account deleted successfully!";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting account: {ex.Message}";
            }

            return RedirectToPage("List");
        }

        private void LoadAccounts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");

            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand(@"SELECT c1.Id, c1.Name, c1.ParentId, c2.Name as ParentName 
                                      FROM ChartOfAccounts c1
                                      LEFT JOIN ChartOfAccounts c2 ON c1.ParentId = c2.Id
                                      ORDER BY c1.Name", con);
            con.Open();
            var reader = cmd.ExecuteReader();

            FlatAccounts = new List<ChartOfAccount>(); // Maintain flat list for table view
            while (reader.Read())
            {
                var account = new ChartOfAccount
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    ParentId = reader["ParentId"] as int?,
                    ParentName = reader["ParentName"] as string
                };
                FlatAccounts.Add(account);
            }
        }

        private void BuildAccountTree()
        {
            // First create a dictionary for quick lookup
            var accountLookup = FlatAccounts.ToDictionary(a => a.Id);

            // Build hierarchical structure
            Accounts = new List<ChartOfAccount>();

            foreach (var account in FlatAccounts)
            {
                if (account.ParentId.HasValue && accountLookup.TryGetValue(account.ParentId.Value, out var parentAccount))
                {
                    parentAccount.Children.Add(account);
                }
                else
                {
                    Accounts.Add(account); // This is a root account
                }
            }

            // Sort children alphabetically
            foreach (var account in FlatAccounts)
            {
                account.Children = account.Children.OrderBy(c => c.Name).ToList();
            }

            // Sort root accounts alphabetically
            Accounts = Accounts.OrderBy(a => a.Name).ToList();
        }



        public IActionResult OnGetExportToExcel()
        {
      
            // Get data from database (using your existing LoadAccounts method)
            LoadAccounts();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ChartOfAccounts");

                // Add headers with styling
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Account Name";
                worksheet.Cells[1, 3].Value = "Parent Account";
                worksheet.Cells[1, 4].Value = "Account Type";

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // Add data rows
                int row = 2;
                foreach (var account in FlatAccounts) // Using your existing FlatAccounts property
                {
                    worksheet.Cells[row, 1].Value = account.Id;
                    worksheet.Cells[row, 2].Value = account.Name;
                    worksheet.Cells[row, 3].Value = account.ParentName ?? "N/A";
                    worksheet.Cells[row, 4].Value = GetAccountTypeBadge(account.Name);
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set some basic styling for the data
                using (var range = worksheet.Cells[2, 1, row - 1, 4])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Return the Excel file
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"ChartOfAccounts_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(package.GetAsByteArray(), contentType, fileName);
            }
        }

        // Add this helper method (similar to your view function)
        private string GetAccountTypeBadge(string accountName)
        {
            if (accountName.Contains("Asset", StringComparison.OrdinalIgnoreCase))
                return "Asset";
            if (accountName.Contains("Liability", StringComparison.OrdinalIgnoreCase))
                return "Liability";
            if (accountName.Contains("Equity", StringComparison.OrdinalIgnoreCase))
                return "Equity";
            if (accountName.Contains("Income", StringComparison.OrdinalIgnoreCase))
                return "Income";
            if (accountName.Contains("Expense", StringComparison.OrdinalIgnoreCase))
                return "Expense";
            return "Account";
        }

    }
}