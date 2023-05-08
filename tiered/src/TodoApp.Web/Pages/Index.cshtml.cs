using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace TodoApp.Web.Pages;

public class IndexModel : TodoAppPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
