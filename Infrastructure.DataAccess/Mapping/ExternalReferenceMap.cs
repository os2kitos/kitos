using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
   public class ExternalReferenceMap : EntityMap<ExternalReference>
    {
        public ExternalReferenceMap() { 
         this.ToTable("ExternalReferences");
            this.Property(t => t.ItProject_Id).HasColumnName("ItProject_Id").IsOptional();
            this.Property(t => t.Itcontract_Id).HasColumnName("ItContract_Id").IsOptional();
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id").IsOptional();
        }
    }
}
