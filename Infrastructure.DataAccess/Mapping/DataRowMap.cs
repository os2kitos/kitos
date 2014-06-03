using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowMap : EntityMap<DataRow>
    {
        public DataRowMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("DataRow");

            this.HasRequired(t => t.DataType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.DataTypeId);

            this.HasRequired(t => t.ItSystem)
                .WithMany(d => d.DataRows)
                .HasForeignKey(t => t.ItSystemId)
                .WillCascadeOnDelete(true);

        }
    }
}