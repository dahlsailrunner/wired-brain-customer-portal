using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WiredBrain.Identity.Models;

namespace WiredBrain.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<WiredBrainUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
