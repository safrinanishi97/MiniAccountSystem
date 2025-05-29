using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountSystem.Services;

namespace MiniAccountSystem.Pages.ChartOfAccounts
{
    public class IndexModel : PageModel
    {
        //private readonly PermissionService _permissionService;
        //private readonly IConfiguration _configuration;

        //public IndexModel(PermissionService permissionService, IConfiguration configuration)
        //{
        //    _permissionService = permissionService;
        //    _configuration = configuration;
        //}
        private readonly PermissionService _permissionService;
        private readonly UserManager<IdentityUser> _userManager;
        public int TotalAccounts { get; set; }
        public IndexModel(PermissionService permissionService, UserManager<IdentityUser> userManager)
        {
            _permissionService = permissionService;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            string role = User.IsInRole("Admin") ? "Admin" :
                          User.IsInRole("Accountant") ? "Accountant" :
                          User.IsInRole("Viewer") ? "Viewer" : "";

            if (!_permissionService.HasAccess(role, "ChartOfAccounts"))
            {
                return RedirectToPage("/AccessDenied");
            }

            // Otherwise continue
            return Page();
        }


        //public IActionResult OnGet()
        //{
        //    string role = User.IsInRole("Admin") ? "Admin" :
        //                  User.IsInRole("Accountant") ? "Accountant" :
        //                  User.IsInRole("Viewer") ? "Viewer" : "";

        //    if (!_permissionService.HasAccess(role, "ChartOfAccounts"))
        //    {
        //        return RedirectToPage("/AccessDenied");
        //    }

        //    // Get total accounts count
        //    string connectionString = _configuration.GetConnectionString("DefaultConnection")
        //    ?? throw new ArgumentNullException("Connection string is missing!");
        //    using (var con = new SqlConnection(connectionString))
        //    {
        //        var cmd = new SqlCommand("SELECT COUNT(*) FROM ChartOfAccounts", con);
        //        con.Open();
        //        TotalAccounts = (int)cmd.ExecuteScalar();
        //    }

        //    return Page();
        //}
    }
}
