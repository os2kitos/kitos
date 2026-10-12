using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ConfigMap : EntityMap<Config>
    {
        public override void Configure(EntityTypeBuilder<Config> builder)
        {
            base.Configure(builder);
            builder.ToTable("Config");
            builder.Property(t => t.Id).HasColumnName("Id");

            builder.HasOne(t => t.Organization)
                .WithOne(t => t.Config)
                .HasForeignKey<Config>(c => c.Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
