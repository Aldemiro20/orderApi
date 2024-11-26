using Microsoft.EntityFrameworkCore;
using orderApi.Models;
using orderApi.Data.Map;
using StackExchange.Redis;

namespace orderApi.Data
{
    public class OrderDBContext:DbContext
    {

        public OrderDBContext(DbContextOptions<OrderDBContext> options) : base(options)
        {

        }
        public DbSet<OrderModel> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderMap());

            base.OnModelCreating(modelBuilder);

        }
    }
}
