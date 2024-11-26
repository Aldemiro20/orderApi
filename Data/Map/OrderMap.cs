using orderApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace orderApi.Data.Map
{
    public class OrderMap : IEntityTypeConfiguration<OrderModel>
    {
        public void Configure(EntityTypeBuilder<OrderModel> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);
            

        }
    }
}
