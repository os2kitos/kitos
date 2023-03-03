using System;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents that an interface exposes some data.
    /// The interface will have a datarow for each piece of data that the
    /// interface exposes.
    /// </summary>
    public class DataRow : Entity, ISystemModule, IHasUuid
    {
        public DataRow()
        {
            Uuid = Guid.NewGuid();
        }

        public int ItInterfaceId { get; set; }
        /// <summary>
        /// The interface which exposes the data
        /// </summary>
        public virtual ItInterface ItInterface { get; set; }

        public int? DataTypeId { get; set; }
        /// <summary>
        /// The type of the data
        /// </summary>
        public virtual DataType DataType { get; set; }

        /// <summary>
        /// Description/name of the data
        /// </summary>
        public string Data { get; set; }

        public Guid Uuid { get; set; }
    }
}
