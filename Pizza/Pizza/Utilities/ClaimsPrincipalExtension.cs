using System.Security.Claims;
using Pizza_Demo.Exceptions;

namespace Pizza.Utilities
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetId(this ClaimsPrincipal self)
        {
            return self.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static bool IsAdmin(this ClaimsPrincipal self)
        {
            return self.IsInRole("Administrator");
        }

        public static void EnsurePermission(this ClaimsPrincipal self, string id)
        {
            if (!self.IsAdmin() && id != self.GetId())
            {
                throw new ServiceException(ServiceExceptionType.Forbidden);
            }
        }


    }
}
