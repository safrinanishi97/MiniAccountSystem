using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization; // Add this using directive

namespace MiniAccountSystem.Pages.Admin
{
    // Apply the Authorize attribute here, specifying the "Admin" role
    [Authorize(Roles = "Admin")]
    public class AssignModuleAccessModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AssignModuleAccessModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string SelectedRole { get; set; }

        [BindProperty]
        public string SelectedModule { get; set; }

        public List<SelectListItem> Roles { get; set; }
        public List<SelectListItem> Modules { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            Roles = new List<SelectListItem>
            {
                new SelectListItem("Admin", "Admin"),
                new SelectListItem("Accountant", "Accountant"),
                new SelectListItem("Viewer", "Viewer")
            };

            Modules = new List<SelectListItem>
            {
                new SelectListItem("Chart Of Accounts", "ChartOfAccounts"),
                new SelectListItem("Voucher Entry", "VoucherEntry"),
                new SelectListItem("Reports", "Reports")
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new("sp_AssignUserAccess", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@RoleName", SelectedRole);
            cmd.Parameters.AddWithValue("@ModuleName", SelectedModule);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            con.Close();

            Message = "Module access assigned successfully.";
            OnGet(); // Reload dropdowns
            return Page();
        }
    }
}