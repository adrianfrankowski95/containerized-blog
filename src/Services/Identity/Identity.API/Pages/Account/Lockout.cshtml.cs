#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;


[AllowAnonymous]
public class LockoutModel : PageModel
{

    public void OnGet()
    {
    }
}
