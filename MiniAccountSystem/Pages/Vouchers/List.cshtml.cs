using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountSystem.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Data;
namespace MiniAccountSystem.Pages.Vouchers
{
    public class ListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("Connection string is missing!");

            using var con = new SqlConnection(connectionString);
            if (string.IsNullOrEmpty(connectionString))
            {
                ErrorMessage = "Database connection string is missing or empty.";
                return;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand("sp_GetVouchers", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Vouchers.Add(new Voucher
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                ReferenceNo = reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
                                TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit")),
                                // Assuming CreatedDate is added and available
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ErrorMessage = $"Database error loading vouchers: {ex.Message}";
                // Log the exception (e.g., using ILogger)
                Console.WriteLine($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // public class Voucher 
    // {
    //     public int Id { get; set; }
    //     public string VoucherType { get; set; }
    //     public string ReferenceNo { get; set; }
    //     public DateTime VoucherDate { get; set; }
    //     public decimal TotalDebit { get; set; }
    //     public decimal TotalCredit { get; set; }
    //     public DateTime CreatedDate { get; set; } 
    // }
}