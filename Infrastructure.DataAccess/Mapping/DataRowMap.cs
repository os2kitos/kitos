using Core.DomainModel.ItSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowMap : EntityMap<DataRow>
    {
        public override void Configure(EntityTypeBuilder<DataRow> builder)
        {
            base.Configure(builder);
            builder.ToTable("DataRow");

            builder.HasOne(t => t.DataType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.DataTypeId);

            builder.HasOne(t => t.ItInterface)
                .WithMany(d => d.DataRows)
                .HasForeignKey(t => t.ItInterfaceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_uuid");
        }
    }
}
