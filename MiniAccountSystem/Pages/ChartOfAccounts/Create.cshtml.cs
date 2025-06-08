using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public List<ChartOfAccount> ParentAccounts { get; set; }

        public IActionResult OnGet()
        {
            // Add this authorization check at the beginning
            if (User.IsInRole("Viewer") || User.IsInRole("Accountant"))
            {
                return RedirectToPage("/AccessDenied");
            }

            // Rest of your existing OnGet code remains the same
            ParentAccounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");
            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand(@"SELECT Id, Name FROM ChartOfAccounts 
                         WHERE ParentId IS NULL
                         ORDER BY Name", con);
            con.Open();
            var reader = cmd.ExecuteReader();
            ParentAccounts = new();
            while (reader.Read())
            {
                ParentAccounts.Add(new ChartOfAccount
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString()
                });
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (User.IsInRole("Viewer") || User.IsInRole("Accountant"))
            {
                return RedirectToPage("/AccessDenied");
            }

            if (!ModelState.IsValid)
                return Page();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
     ?? throw new ArgumentNullException("Connection string is missing!");
            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "CREATE");
            cmd.Parameters.AddWithValue("@AccountName", Account.Name);
            cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();

            TempData["SuccessMessage"] = "Account created successfully!";
            return RedirectToPage("List");
        }
    }

}
