
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
            if (User.IsInRole("Viewer") || User.IsInRole("Accountant"))
            {
                return RedirectToPage("/AccessDenied");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    var cmd = new SqlCommand("SELECT Id, Name, ParentId FROM ChartOfAccounts WHERE Id = @Id", conn);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Account = new ChartOfAccount
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                ParentId = reader["ParentId"] as int?
                            };
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    var cmd = new SqlCommand(
                        @"SELECT Id, Name FROM ChartOfAccounts 
                          WHERE Id != @Id AND (ParentId IS NULL OR ParentId != @Id)
                          ORDER BY Name", conn);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
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
                }

                return Page();
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
                return RedirectToPage("List");
            }
        }

        public IActionResult OnPost()
        {
            if (User.IsInRole("Viewer") || User.IsInRole("Accountant"))
            {
                return RedirectToPage("/AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

            try
            {
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
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Error updating account: {ex.Message}";
                return Page();
            }
        }
    }
}
