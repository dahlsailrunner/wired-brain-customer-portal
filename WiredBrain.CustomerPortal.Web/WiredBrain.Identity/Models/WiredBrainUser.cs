using Microsoft.AspNetCore.Identity;

namespace WiredBrain.Identity.Models
{
    public class WiredBrainUser : IdentityUser
    {
        public int LoyaltyNumber { get; set; }
    }
}
