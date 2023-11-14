using Microsoft.EntityFrameworkCore;

namespace WebApplication_Hierarchical_Directory_System.Models
{
    public class DirectoryContext : DbContext
    {
        public DirectoryContext(DbContextOptions options) : base(options) { }
        public virtual DbSet<MyDirectory> Directories { get; set; } = null!;
    }
}
