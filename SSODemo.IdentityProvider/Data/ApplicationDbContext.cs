using Microsoft.EntityFrameworkCore;

namespace SSODemo.IdentityProvider.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
