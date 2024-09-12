using Microsoft.EntityFrameworkCore;

namespace Arkad.Server.Models._Context
{
    public class AppDbSet : DbContext
    {
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
