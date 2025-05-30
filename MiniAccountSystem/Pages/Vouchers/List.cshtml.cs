using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountSystem.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
namespace MiniAccountSystem.Pages.Vouchers
{



    public class ListModel : PageModel
    {
        private readonly IConfiguration _config;

        public ListModel(IConfiguration config)
        {
            _config = config;
        }

        public List<VoucherListDto> Vouchers { get; set; }

        public void OnGet()
        {
            Vouchers = GetAllVouchersFromDB();
        }

        public IActionResult OnPostDelete(int id)
        {
            DeleteVoucherById(id);
            return RedirectToPage();
        }

        private List<VoucherListDto> GetAllVouchersFromDB()
        {
            var list = new List<VoucherListDto>();
            var connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllVouchersWithDetails", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            var reader = cmd.ExecuteReader();

            var lookup = new Dictionary<int, VoucherListDto>();

            while (reader.Read())
            {
                int voucherId = Convert.ToInt32(reader["VoucherId"]);

                if (!lookup.TryGetValue(voucherId, out var voucher))
                {
                    voucher = new VoucherListDto
                    {
                        VoucherId = voucherId,
                        VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                        ReferenceNo = reader["ReferenceNo"] == DBNull.Value ? string.Empty : reader["ReferenceNo"].ToString(),
                        VoucherType = reader["VoucherType"] == DBNull.Value ? string.Empty : reader["VoucherType"].ToString(),

                    };
                    lookup.Add(voucherId, voucher);
                }

                voucher.VoucherDetails.Add(new VoucherDetailListDto
                {
                    VoucherDetailId = Convert.ToInt32(reader["VoucherDetailId"]),
                    AccountId = Convert.ToInt32(reader["AccountId"]),
                    AccountName = reader["AccountName"] == DBNull.Value ? string.Empty : reader["AccountName"].ToString(),
                    DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                    CreditAmount = Convert.ToDecimal(reader["CreditAmount"])
                });
            }

            return lookup.Values.ToList();
        }

        private void DeleteVoucherById(int voucherId)
        {
            var connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_DeleteVoucher", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VoucherId", voucherId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }













    //public class ListModel : PageModel
    //{
    //    private readonly IConfiguration _configuration;

    //    public ListModel(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }

    //    public List<Voucher> Vouchers { get; set; } = new List<Voucher>();

    //    public async Task OnGetAsync()
    //    {
    //        string connectionString = _configuration.GetConnectionString("DefaultConnection")
    //        ?? throw new ArgumentNullException("Connection string is missing!"); // Get your connection string

    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            using (SqlCommand command = new SqlCommand("sp_GetVouchers", connection)) // Stored procedure to fetch vouchers
    //            {
    //                command.CommandType = CommandType.StoredProcedure;
    //                // Add parameters if you want to filter vouchers (e.g., by date range, type)
    //                // command.Parameters.AddWithValue("@StartDate", DateTime.Today.AddMonths(-1));
    //                // command.Parameters.AddWithValue("@EndDate", DateTime.Today);

    //                await connection.OpenAsync();
    //                using (SqlDataReader reader = await command.ExecuteReaderAsync())
    //                {
    //                    while (reader.Read())
    //                    {
    //                        Vouchers.Add(new Voucher
    //                        {
    //                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
    //                            VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
    //                            ReferenceNo = reader.GetString(reader.GetOrdinal("ReferenceNo")),
    //                            VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
    //                            TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
    //                            TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit")),
    //                            CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
    //                            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy"))
    //                        });
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}