using Core.DomainModel.UIConfiguration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class CustomizedUINodeMap : IEntityTypeConfiguration<CustomizedUINode>
    {
        public void Configure(EntityTypeBuilder<CustomizedUINode> builder)
        {
            builder.ToTable("CustomizedUiNodes");

            builder.Property(x => x.Key)
                .IsRequired();
            builder.Property(x => x.Enabled)
                .IsRequired();
            
            builder.HasOne(t => t.UiModuleCustomization)
                .WithMany(t => t.Nodes)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
