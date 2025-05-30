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

        public async Task OnGetAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing!"); // Get your connection string

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetVouchers", connection)) // Stored procedure to fetch vouchers
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // Add parameters if you want to filter vouchers (e.g., by date range, type)
                    // command.Parameters.AddWithValue("@StartDate", DateTime.Today.AddMonths(-1));
                    // command.Parameters.AddWithValue("@EndDate", DateTime.Today);

                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            Vouchers.Add(new Voucher
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                ReferenceNo = reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
                                TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit")),
                                CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy"))
                            });
                        }
                    }
                }
            }
        }
    }

}