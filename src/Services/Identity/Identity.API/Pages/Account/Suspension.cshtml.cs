using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class SuspensionModel : PageModel
{
    public Instant SuspendedUntil { get; set; }
    public IActionResult OnGet(Instant suspendedUntil)
    {
        if (suspendedUntil == null || suspendedUntil == default)
            return RedirectToPage("/Index");

        SuspendedUntil = suspendedUntil;

        return Page();
    }
}
