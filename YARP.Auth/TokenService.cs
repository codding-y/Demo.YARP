using System.Security.Claims;

namespace YARP.Auth
{
    internal class TokenService
    {
        internal Task<string> GetAuthTokenAsync(ClaimsPrincipal? user)
        {
            if (string.Equals("codding-y", user?.Identity?.Name))
            {
                return Task.FromResult(Guid.NewGuid().ToString());
            }
            return Task.FromResult("");
        }
    }
}
