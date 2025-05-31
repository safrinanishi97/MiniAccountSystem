using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    public class EditModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EditModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public List<ChartOfAccount> ParentAccounts { get; set; }

        public IActionResult OnGet(int id)
        {
            ParentAccounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");

            // Load the account to edit
            using (var con = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand("SELECT Id, Name, ParentId FROM ChartOfAccounts WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Account = new ChartOfAccount
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        ParentId = reader["ParentId"] != DBNull.Value ? (int?)reader["ParentId"] : null
                    };
                }
                else
                {
                    return NotFound();
                }
            }

            // Load parent accounts (excluding the current account and its descendants)
            using (var con = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand(@"SELECT Id, Name FROM ChartOfAccounts 
                                         WHERE Id != @Id AND (ParentId IS NULL OR ParentId != @Id)
                                         ORDER BY Name", con);
                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                var reader = cmd.ExecuteReader();
                ParentAccounts = new List<ChartOfAccount>();
                while (reader.Read())
                {
                    ParentAccounts.Add(new ChartOfAccount
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString()
                    });
                }
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");

            using (var con = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@AccountId", Account.Id);
                cmd.Parameters.AddWithValue("@AccountName", Account.Name);
                cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Account updated successfully!";
            return RedirectToPage("List");
        }
    }
}
