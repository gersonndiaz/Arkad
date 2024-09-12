using Microsoft.EntityFrameworkCore;

namespace Arkad.Server.Models._Context
{
    public class AppDbSet : DbContext
    {
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
