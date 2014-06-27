using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents that an interface (ItSystem) exposes some data.
    /// The interface will have a datarow for each piece of data that the
    /// interface exposes.
    /// </summary>
    public class DataRow : Entity
    {
        public DataRow()
        {
            this.Usages = new List<DataRowUsage>();
        }

        public int ItSystemId { get; set; }
        /// <summary>
        /// The interface which exposes the data
        /// </summary>
        public virtual ItSystem ItSystem { get; set; }

        public int? DataTypeId { get; set; }
        /// <summary>
        /// The type of the data
        /// </summary>
        public virtual DataType DataType { get; set; }

        /// <summary>
        /// Description/name of the data
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// Local usages of this exposition
        /// </summary>
        public virtual ICollection<DataRowUsage> Usages { get; set; }
    }
}
