using System.Linq;
using System.Security.Claims;

namespace AngularMaterialApi.Extension
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var currentUser = user.FindFirst("preferred_username").Value;
            return currentUser;
        }
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
