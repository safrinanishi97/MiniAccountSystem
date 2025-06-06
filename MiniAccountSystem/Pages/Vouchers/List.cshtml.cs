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
                        CreatedBy = reader["CreatedBy"]?.ToString() ?? "System",
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),

                        UpdatedBy = reader["UpdatedBy"] == DBNull.Value ? null : reader["UpdatedBy"].ToString(),
                        UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedDate"])
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

        public IActionResult OnGetExportToExcel()
        {
            var vouchers = GetAllVouchersFromDB();

            using (var package = new ExcelPackage())
            {
                // Create a worksheet for voucher summary
                var summarySheet = package.Workbook.Worksheets.Add("Voucher Summary");

                // Add headers for summary sheet (updated with new columns)
                summarySheet.Cells[1, 1].Value = "Voucher ID";
                summarySheet.Cells[1, 2].Value = "Date";
                summarySheet.Cells[1, 3].Value = "Reference No";
                summarySheet.Cells[1, 4].Value = "Type";
                summarySheet.Cells[1, 5].Value = "Created By";
                summarySheet.Cells[1, 6].Value = "Created At";
                summarySheet.Cells[1, 7].Value = "Total Debit";
                summarySheet.Cells[1, 8].Value = "Total Credit";
                summarySheet.Cells[1, 9].Value = "Updated By";
                summarySheet.Cells[1, 10].Value = "Updated At";

                // Style summary headers (updated range)
                using (var range = summarySheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkSlateBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Add voucher summary data (updated with new fields)
                int summaryRow = 2;
                foreach (var voucher in vouchers)
                {
                    summarySheet.Cells[summaryRow, 1].Value = voucher.VoucherId;
                    summarySheet.Cells[summaryRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
                    summarySheet.Cells[summaryRow, 3].Value = voucher.ReferenceNo;
                    summarySheet.Cells[summaryRow, 4].Value = voucher.VoucherType;
                    summarySheet.Cells[summaryRow, 5].Value = FormatCreatedBy(voucher.CreatedBy);
                    summarySheet.Cells[summaryRow, 6].Value = voucher.CreatedDate.ToString("dd-MMM-yyyy HH:mm");
                    summarySheet.Cells[summaryRow, 7].Value = voucher.VoucherDetails.Sum(d => d.DebitAmount);
                    summarySheet.Cells[summaryRow, 8].Value = voucher.VoucherDetails.Sum(d => d.CreditAmount);
                    summarySheet.Cells[summaryRow, 9].Value =
                        string.IsNullOrWhiteSpace(voucher.UpdatedBy) ? "N/A" : FormatCreatedBy(voucher.UpdatedBy);

                    summarySheet.Cells[summaryRow, 10].Value =
                        voucher.UpdatedDate.HasValue ? voucher.UpdatedDate.Value.ToString("dd-MMM-yyyy HH:mm") : "N/A";

                    // Format currency cells (updated column indexes)
                    summarySheet.Cells[summaryRow, 7].Style.Numberformat.Format = "#,##0.00";
                    summarySheet.Cells[summaryRow, 8].Style.Numberformat.Format = "#,##0.00";

                    // Center align all cells
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Add borders
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    summarySheet.Cells[summaryRow, 1, summaryRow, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    summaryRow++;
                }

                // Auto-fit columns for summary sheet
                summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();

                // Create a worksheet for voucher details
                var detailsSheet = package.Workbook.Worksheets.Add("Voucher Details");

                // Add headers for details sheet (updated with new columns)
                detailsSheet.Cells[1, 1].Value = "Voucher ID";
                detailsSheet.Cells[1, 2].Value = "Date";
                detailsSheet.Cells[1, 3].Value = "Reference No";
                detailsSheet.Cells[1, 4].Value = "Type";
                detailsSheet.Cells[1, 5].Value = "Created By";
                detailsSheet.Cells[1, 6].Value = "Created At";
                detailsSheet.Cells[1, 7].Value = "Account ID";
                detailsSheet.Cells[1, 8].Value = "Account Name";
                detailsSheet.Cells[1, 9].Value = "Debit";
                detailsSheet.Cells[1, 10].Value = "Credit";
                detailsSheet.Cells[1, 11].Value = "Updated By";
                detailsSheet.Cells[1, 12].Value = "Updated At";


                // Style details headers (updated range)
                using (var range = detailsSheet.Cells[1, 1, 1, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Add voucher details data (updated with new fields)
                int detailsRow = 2;
                foreach (var voucher in vouchers)
                {
                    foreach (var detail in voucher.VoucherDetails)
                    {
                        detailsSheet.Cells[detailsRow, 1].Value = voucher.VoucherId;
                        detailsSheet.Cells[detailsRow, 2].Value = voucher.VoucherDate.ToString("dd-MMM-yyyy");
                        detailsSheet.Cells[detailsRow, 3].Value = voucher.ReferenceNo;
                        detailsSheet.Cells[detailsRow, 4].Value = voucher.VoucherType;
                        detailsSheet.Cells[detailsRow, 5].Value = FormatCreatedBy(voucher.CreatedBy);
                        detailsSheet.Cells[detailsRow, 6].Value = voucher.CreatedDate.ToString("dd-MMM-yyyy HH:mm");
                        detailsSheet.Cells[detailsRow, 7].Value = detail.AccountId;
                        detailsSheet.Cells[detailsRow, 8].Value = detail.AccountName;
                        detailsSheet.Cells[detailsRow, 9].Value = detail.DebitAmount;
                        detailsSheet.Cells[detailsRow, 10].Value = detail.CreditAmount;
                        detailsSheet.Cells[detailsRow, 11].Value = string.IsNullOrWhiteSpace(voucher.UpdatedBy) ? "N/A" : FormatCreatedBy(voucher.UpdatedBy);

                        detailsSheet.Cells[detailsRow, 12].Value = voucher.UpdatedDate.HasValue ? voucher.UpdatedDate.Value.ToString("dd-MMM-yyyy HH:mm") : "N/A";

                        // Format currency cells (updated column indexes)
                        detailsSheet.Cells[detailsRow, 9].Style.Numberformat.Format = "#,##0.00";
                        detailsSheet.Cells[detailsRow, 10].Style.Numberformat.Format = "#,##0.00";


                        // Center align all cells
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        // Add borders
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        detailsSheet.Cells[detailsRow, 1, detailsRow, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        // Color coding for debit/credit
                        detailsSheet.Cells[detailsRow, 9].Style.Font.Color.SetColor(Color.Green);
                        detailsSheet.Cells[detailsRow, 10].Style.Font.Color.SetColor(Color.Red);


                        detailsRow++;
                    }
                }

                // Auto-fit columns for details sheet
                detailsSheet.Cells[detailsSheet.Dimension.Address].AutoFitColumns();

                //// Add conditional formatting for debit/credit columns (updated column indexes)
                //var debitColumn = detailsSheet.Cells[2, 9, detailsRow - 1, 9];
                //var creditColumn = detailsSheet.Cells[2, 10, detailsRow - 1, 10];

                //debitColumn.Style.Font.Color.SetColor(Color.Green);
                //creditColumn.Style.Font.Color.SetColor(Color.Red);

                // Return the Excel file
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"Vouchers_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(package.GetAsByteArray(), contentType, fileName);
            }


        }

        private string FormatCreatedBy(string createdBy)
        {
            if (createdBy.Contains("("))
            {
                var parts = createdBy.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                return $"{parts[0].Trim()} ({parts[1].Trim()})";
            }
            return createdBy;
        }

    }

}