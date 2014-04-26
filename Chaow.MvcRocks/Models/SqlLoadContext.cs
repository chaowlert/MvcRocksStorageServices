using System.Data.Entity;

namespace Chaow.MvcRocks.Models
{
    public class SqlLoadContext : DbContext
    {
        public DbSet<LoadItem> UnorderedSet { get; set; }
        public DbSet<LoadItem2> OrderedSet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoadItem>().HasKey(item => new {item.PartitionKey, item.RowKey});
            modelBuilder.Entity<LoadItem2>().HasKey(item => new { item.PartitionKey, item.RowKey });
        }
    }
}