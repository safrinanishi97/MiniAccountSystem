using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Models;
using System.Data;
using System.Text.Encodings.Web;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    [ValidateAntiForgeryToken]
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
            Accounts = LoadAccounts() ?? new List<ChartOfAccount>();
        }
       
        //public async Task<IActionResult> OnPostDeleteAsync(int id)
        //{
        //    try
        //    {
        //        string connectionString = _configuration.GetConnectionString("DefaultConnection")
        //            ?? throw new ArgumentNullException("Connection string is missing!");

        //        using var con = new SqlConnection(connectionString);
        //        var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@Action", "DELETE");
        //        cmd.Parameters.AddWithValue("@AccountId", id);

        //        await con.OpenAsync();
        //        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        //        if (rowsAffected > 0)
        //        {
        //            TempData["SuccessMessage"] = "Account deleted successfully";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = "Account not found or could not be deleted";
        //        }
        //    }
        //    catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint violation
        //    {
        //        TempData["ErrorMessage"] = "Cannot delete this account because it has child accounts or transactions.";
        //    }
        //    catch (SqlException ex)
        //    {
        //        TempData["ErrorMessage"] = $"Database error: {ex.Message}";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = $"Error: {ex.Message}";
        //    }

        //    return RedirectToPage();
        //}
        public async Task<IActionResult> OnPostDeleteAsync(int id)
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

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                TempData["SuccessMessage"] = "Account deleted successfully";
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["ErrorMessage"] = "Cannot delete: Account has child records";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting account: {ex.Message}";
            }

            return RedirectToPage();
        }

        private List<ChartOfAccount> LoadAccounts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string is missing!");

            var accounts = new List<ChartOfAccount>();

            using var con = new SqlConnection(connectionString);
            var cmd = new SqlCommand("SELECT Id, Name, ParentId FROM ChartOfAccounts", con);

            con.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                accounts.Add(new ChartOfAccount
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    ParentId = reader["ParentId"] as int?
                });
            }

            return accounts;
        }
    }


}
