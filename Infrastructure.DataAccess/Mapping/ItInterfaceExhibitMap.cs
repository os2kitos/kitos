using Core.DomainModel.ItSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceExhibitMap : EntityMap<ItInterfaceExhibit>
    {
        public override void Configure(EntityTypeBuilder<ItInterfaceExhibit> builder)
        {
            base.Configure(builder);
            builder.ToTable("Exhibit");

            builder.HasOne(t => t.ItInterface)
                .WithOne(t => t.ExhibitedBy)
                .HasForeignKey<ItInterfaceExhibit>(t => t.Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
