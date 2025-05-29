using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public List<ChartOfAccount> Parents { get; set; } = new();

        private readonly string connectionString = "DefaultConnection";

        public void OnGet(int id)
        {
            // Load Account to Edit
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
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
                }

                // Load Parents excluding current account to avoid cyclic parent
                SqlCommand cmdParents = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE Id <> @Id", conn);
                cmdParents.Parameters.AddWithValue("@Id", id);
                using (var reader = cmdParents.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Parents.Add(new ChartOfAccount
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                // Reload Parents for dropdown on error
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmdParents = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE Id <> @Id", conn);
                    cmdParents.Parameters.AddWithValue("@Id", Account.Id);
                    using (var reader = cmdParents.ExecuteReader())
                    {
                        Parents.Clear();
                        while (reader.Read())
                        {
                            Parents.Add(new ChartOfAccount
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
                return Page();
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@Id", Account.Id);
                cmd.Parameters.AddWithValue("@Name", Account.Name);
                cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("Index");
        }
    }
}
