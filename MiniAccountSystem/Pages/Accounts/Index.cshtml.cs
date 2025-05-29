using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountSystem.Services;

namespace MiniAccountSystem.Pages.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly PermissionService _permissionService;

        public IndexModel(PermissionService permissionService)
        {
            _permissionService = permissionService;
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

            return Page();
        }
    }
}
