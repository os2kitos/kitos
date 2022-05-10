using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.UIConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class UIVisibilityConfigurationMap : EntityTypeConfiguration<UIVisibilityConfiguration>
    {
        public UIVisibilityConfigurationMap()
        {
            HasKey(x => x.Id);

            Property(x => x.Key)
                .IsRequired();
            Property(x => x.Module)
                .IsRequired();
            Property(x => x.Enabled)
                .IsRequired();
            Property(x => x.OrganizationId)
                .IsRequired();
            
            HasRequired(t => t.Organization)
                .WithMany(t => t.UiVisibilityConfigurations)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);
        }
    }
}
