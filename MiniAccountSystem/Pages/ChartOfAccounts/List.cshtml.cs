using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    //public class ListModel : PageModel
    //{
    //    private readonly IConfiguration _configuration;

    //    public ListModel(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }
    //    public List<ChartOfAccount> Accounts { get; set; }



    //    public void OnGet()
    //    {
    //        // Get connection string from configuration
    //        string connectionString = _configuration.GetConnectionString("DefaultConnection")
    //        ?? throw new ArgumentNullException("Connection string is missing!");

    //        using var con = new SqlConnection(connectionString);
    //        var cmd = new SqlCommand("SELECT Id, Name, ParentId FROM ChartOfAccounts", con);
    //        con.Open();
    //        var reader = cmd.ExecuteReader();

    //        Accounts = new();
    //        while (reader.Read())
    //        {
    //            Accounts.Add(new ChartOfAccount
    //            {
    //                Id = (int)reader["Id"],
    //                Name = reader["Name"].ToString(),
    //                ParentId = reader["ParentId"] as int?
    //            });
    //        }
    //    }

    //    public IActionResult OnPostDelete(int id)
    //    {
    //        string connectionString = _configuration.GetConnectionString("DefaultConnection")
    //  ?? throw new ArgumentNullException("Connection string is missing!");

    //        using var con = new SqlConnection(connectionString);
    //        var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
    //        cmd.CommandType = CommandType.StoredProcedure;
    //        cmd.Parameters.AddWithValue("@Action", "DELETE");
    //        cmd.Parameters.AddWithValue("@AccountId", id);
    //        con.Open();
    //        cmd.ExecuteNonQuery();

    //        return RedirectToPage();
    //    }
    //}


    public class ListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ChartOfAccount> Accounts { get; set; }

        public void OnGet()
        {
            LoadAccounts();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("Connection string is missing!");

                using var con = new SqlConnection(connectionString);
                var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@AccountId", id);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Account deleted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Account not found or could not be deleted";
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            LoadAccounts();
            return RedirectToPage();
        }

        private void LoadAccounts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
          ?? throw new ArgumentNullException("Connection string is missing!");

            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand("SELECT Id, Name, ParentId FROM ChartOfAccounts", con);

            con.Open();
            var reader = cmd.ExecuteReader();

            Accounts = new List<ChartOfAccount>();
            while (reader.Read())
            {
                Accounts.Add(new ChartOfAccount
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    ParentId = reader["ParentId"] as int?
                });
            }
        }
    }

}
