using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationProject.Areas.Identity.Data;

namespace WebApplicationProject.Areas.Identity.Data;

public class WebApplicationProjectContext : IdentityDbContext<ProjectUser>
{
    public WebApplicationProjectContext(DbContextOptions<WebApplicationProjectContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
