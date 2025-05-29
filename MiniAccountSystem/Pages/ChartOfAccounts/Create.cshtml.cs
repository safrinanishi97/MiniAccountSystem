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

        public void OnGet()
        {
            // Load all accounts to show as potential parents
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing!");
            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts", con);
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
        }

        public IActionResult OnPost()
        {
            using var con = new SqlConnection("DefaultConnection");
            var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "CREATE");
            cmd.Parameters.AddWithValue("@AccountName", Account.Name);
            cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToPage("List");
        }
    }

}
