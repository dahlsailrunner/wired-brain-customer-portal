using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authentication;

namespace WiredBrain.CustomerPortal.Web
{
    public class CustomClaimsTransformer : IClaimsTransformation
    {
        private readonly IDbConnection _userConn;

        public CustomClaimsTransformer(IDbConnection userConn)
        {
            _userConn = userConn;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var id = (ClaimsIdentity)principal.Identity;
            if (!id.IsAuthenticated) return principal;

            var ci = new ClaimsIdentity(id.Claims, id.AuthenticationType, id.NameClaimType, id.RoleClaimType);
            var userId = id.Claims.FirstOrDefault(a => a.Type == "sub")?.Value;

            var loyaltyNum = await _userConn.ExecuteScalarAsync(
                "SELECT LoyaltyNumber FROM AspNetUsers WHERE Id = @userId", new {userId});
            
            ci.AddClaim(new Claim("loyalty",  loyaltyNum.ToString()));
            return new ClaimsPrincipal(ci);
        }
    }
}
