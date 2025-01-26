using mediporta.Entities;
using Microsoft.EntityFrameworkCore;


namespace mediporta.Context
{
    public class TagsDbContext(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Tag> Tags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }   
}
