using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountSystem.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
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

        //public IActionResult OnGetExportToExcel()
        //{

        //    var vouchers = GetAllVouchersFromDB();

        //    using (var package = new ExcelPackage())
        //    {
        //        // Create a worksheet for voucher summary
        //        var summarySheet = package.Workbook.Worksheets.Add("Voucher Summary");

        //        // Add headers for summary sheet
        //        summarySheet.Cells[1, 1].Value = "Voucher ID";
        //        summarySheet.Cells[1, 2].Value = "Date";
        //        summarySheet.Cells[1, 3].Value = "Reference No";
        //        summarySheet.Cells[1, 4].Value = "Type";
        //        summarySheet.Cells[1, 5].Value = "Total Debit";
        //        summarySheet.Cells[1, 6].Value = "Total Credit";

        //        // Style summary headers
        //        using (var range = summarySheet.Cells[1, 1, 1, 6])
        //        {
        //            range.Style.Font.Bold = true;
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        //            range.Style.Font.Color.SetColor(Color.White);
        //        }

        //        // Add voucher summary data
        //        int summaryRow = 2;
        //        foreach (var voucher in vouchers)
        //        {
        //            summarySheet.Cells[summaryRow, 1].Value = voucher.VoucherId;
        //            summarySheet.Cells[summaryRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
        //            summarySheet.Cells[summaryRow, 3].Value = voucher.ReferenceNo;
        //            summarySheet.Cells[summaryRow, 4].Value = voucher.VoucherType;
        //            summarySheet.Cells[summaryRow, 5].Value = voucher.VoucherDetails.Sum(d => d.DebitAmount);
        //            summarySheet.Cells[summaryRow, 6].Value = voucher.VoucherDetails.Sum(d => d.CreditAmount);

        //            // Format currency cells
        //            summarySheet.Cells[summaryRow, 5].Style.Numberformat.Format = "#,##0.00";
        //            summarySheet.Cells[summaryRow, 6].Style.Numberformat.Format = "#,##0.00";

        //            summaryRow++;
        //        }

        //        // Auto-fit columns for summary sheet
        //        summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();

        //        // Create a worksheet for voucher details
        //        var detailsSheet = package.Workbook.Worksheets.Add("Voucher Details");

        //        // Add headers for details sheet
        //        detailsSheet.Cells[1, 1].Value = "Voucher ID";
        //        detailsSheet.Cells[1, 2].Value = "Date";
        //        detailsSheet.Cells[1, 3].Value = "Reference No";
        //        detailsSheet.Cells[1, 4].Value = "Type";
        //        detailsSheet.Cells[1, 5].Value = "Account ID";
        //        detailsSheet.Cells[1, 6].Value = "Account Name";
        //        detailsSheet.Cells[1, 7].Value = "Debit";
        //        detailsSheet.Cells[1, 8].Value = "Credit";

        //        // Style details headers
        //        using (var range = detailsSheet.Cells[1, 1, 1, 8])
        //        {
        //            range.Style.Font.Bold = true;
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
        //            range.Style.Font.Color.SetColor(Color.White);
        //        }

        //        // Add voucher details data
        //        int detailsRow = 2;
        //        foreach (var voucher in vouchers)
        //        {
        //            foreach (var detail in voucher.VoucherDetails)
        //            {
        //                detailsSheet.Cells[detailsRow, 1].Value = voucher.VoucherId;
        //                detailsSheet.Cells[detailsRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
        //                detailsSheet.Cells[detailsRow, 3].Value = voucher.ReferenceNo;
        //                detailsSheet.Cells[detailsRow, 4].Value = voucher.VoucherType;
        //                detailsSheet.Cells[detailsRow, 5].Value = detail.AccountId;
        //                detailsSheet.Cells[detailsRow, 6].Value = detail.AccountName;
        //                detailsSheet.Cells[detailsRow, 7].Value = detail.DebitAmount;
        //                detailsSheet.Cells[detailsRow, 8].Value = detail.CreditAmount;

        //                // Format currency cells
        //                detailsSheet.Cells[detailsRow, 7].Style.Numberformat.Format = "#,##0.00";
        //                detailsSheet.Cells[detailsRow, 8].Style.Numberformat.Format = "#,##0.00";

        //                detailsRow++;
        //            }
        //        }

        //        // Auto-fit columns for details sheet
        //        detailsSheet.Cells[detailsSheet.Dimension.Address].AutoFitColumns();

        //        // Add conditional formatting for debit/credit columns
        //        var debitColumn = detailsSheet.Cells[2, 7, detailsRow - 1, 7];
        //        var creditColumn = detailsSheet.Cells[2, 8, detailsRow - 1, 8];

        //        debitColumn.Style.Font.Color.SetColor(Color.Green);
        //        creditColumn.Style.Font.Color.SetColor(Color.Red);

        //        // Return the Excel file
        //        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        var fileName = $"Vouchers_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        //        return File(package.GetAsByteArray(), contentType, fileName);
        //    }
        //}



        public IActionResult OnGetExportToExcel()
        {
            var vouchers = GetAllVouchersFromDB();

            using (var package = new ExcelPackage())
            {
                // Create a worksheet for voucher summary
                var summarySheet = package.Workbook.Worksheets.Add("Voucher Summary");

                // Add headers for summary sheet
                summarySheet.Cells[1, 1].Value = "Voucher ID";
                summarySheet.Cells[1, 2].Value = "Date";
                summarySheet.Cells[1, 3].Value = "Reference No";
                summarySheet.Cells[1, 4].Value = "Type";
                summarySheet.Cells[1, 5].Value = "Total Debit";
                summarySheet.Cells[1, 6].Value = "Total Credit";

                // Style summary headers
                using (var range = summarySheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // Add voucher summary data
                int summaryRow = 2;
                foreach (var voucher in vouchers)
                {
                    summarySheet.Cells[summaryRow, 1].Value = voucher.VoucherId;
                    summarySheet.Cells[summaryRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
                    summarySheet.Cells[summaryRow, 3].Value = voucher.ReferenceNo;
                    summarySheet.Cells[summaryRow, 4].Value = voucher.VoucherType;
                    summarySheet.Cells[summaryRow, 5].Value = voucher.VoucherDetails.Sum(d => d.DebitAmount);
                    summarySheet.Cells[summaryRow, 6].Value = voucher.VoucherDetails.Sum(d => d.CreditAmount);

                    // Format currency cells
                    summarySheet.Cells[summaryRow, 5].Style.Numberformat.Format = "#,##0.00";
                    summarySheet.Cells[summaryRow, 6].Style.Numberformat.Format = "#,##0.00";

                    summaryRow++;
                }

                // Auto-fit columns for summary sheet
                summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();

                // Create a worksheet for voucher details
                var detailsSheet = package.Workbook.Worksheets.Add("Voucher Details");

                // Add headers for details sheet
                detailsSheet.Cells[1, 1].Value = "Voucher ID";
                detailsSheet.Cells[1, 2].Value = "Date";
                detailsSheet.Cells[1, 3].Value = "Account Name";
                detailsSheet.Cells[1, 4].Value = "Debit";
                detailsSheet.Cells[1, 5].Value = "Credit";
                detailsSheet.Cells[1, 6].Value = "Type";
                detailsSheet.Cells[1, 7].Value = "Reference No";

                // Style details headers
                using (var range = detailsSheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // Add voucher details data
                int detailsRow = 2;
                foreach (var voucher in vouchers)
                {
                    foreach (var detail in voucher.VoucherDetails)
                    {
                        detailsSheet.Cells[detailsRow, 1].Value = voucher.VoucherId;
                        detailsSheet.Cells[detailsRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
                        detailsSheet.Cells[detailsRow, 3].Value = detail.AccountName;
                        detailsSheet.Cells[detailsRow, 4].Value = detail.DebitAmount;
                        detailsSheet.Cells[detailsRow, 5].Value = detail.CreditAmount;
                        detailsSheet.Cells[detailsRow, 6].Value = voucher.VoucherType;
                        detailsSheet.Cells[detailsRow, 7].Value = voucher.ReferenceNo;

                        // Format currency cells
                        detailsSheet.Cells[detailsRow, 4].Style.Numberformat.Format = "#,##0.00";
                        detailsSheet.Cells[detailsRow, 5].Style.Numberformat.Format = "#,##0.00";

                        // Color coding
                        detailsSheet.Cells[detailsRow, 4].Style.Font.Color.SetColor(Color.Green);
                        detailsSheet.Cells[detailsRow, 5].Style.Font.Color.SetColor(Color.Red);

                        detailsRow++;
                    }
                }

                // Auto-fit columns for details sheet
                detailsSheet.Cells[detailsSheet.Dimension.Address].AutoFitColumns();

                // Return the Excel file
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"Vouchers_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(package.GetAsByteArray(), contentType, fileName);
            }
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