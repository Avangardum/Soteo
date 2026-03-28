using System.Security.Claims;

namespace Soteo.MasterServer.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension (ClaimsPrincipal self)
    {
        public Guid Id => Guid.Parse(self.FindFirst(Claims.Id)!.Value);
        
        public bool IsPlayer => self.HasTrueClaim(Claims.Player);
        
        public bool IsShard => self.HasTrueClaim(Claims.Shard);
        
        public bool HasTrueClaim(string type) => self.HasClaim(type, "true");
    }
}