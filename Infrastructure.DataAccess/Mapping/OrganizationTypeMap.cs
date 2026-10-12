using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationTypeMap : IEntityTypeConfiguration<OrganizationType>
    {
        public void Configure(EntityTypeBuilder<OrganizationType> builder)
        {
            builder.Property(x => x.Name).IsRequired();
            builder.Property(t => t.Category).IsRequired();

            builder.ToTable("OrganizationTypes");
        }
    }
}