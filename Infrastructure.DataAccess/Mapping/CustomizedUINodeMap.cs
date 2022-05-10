using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.UIConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class CustomizedUINodeMap : EntityTypeConfiguration<CustomizedUINode>
    {
        public CustomizedUINodeMap()
        {
            Property(x => x.Key)
                .IsRequired();
            Property(x => x.Enabled)
                .IsRequired();
            
            HasRequired(t => t.UiModuleCustomization)
                .WithMany(t => t.Nodes)
                .HasForeignKey(d => d.ModuleId)
                .WillCascadeOnDelete(false);
        }
    }
}
