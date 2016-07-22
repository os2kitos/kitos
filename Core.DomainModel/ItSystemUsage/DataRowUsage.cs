﻿using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItSystemUsage
{
    /// <summary>
    /// Helper object for the local usage of a <see cref="DataRow"/>.
    /// When an interface with some DataRows is taken into local usage,
    /// a DataRowUsage is created for each DataRow. This allows for adding details
    /// regarding the usage.
    /// </summary>
    /// TODO the db schema for this is wrong, it needs to be deleted when a contract relation to a ItInterfaceUsage is removed so it needs to somehow attach itself to the relation between ItContract and ItInterfaceUsage
    public class DataRowUsage : ISystemModule
    {
        public int ItSystemUsageId { get; set; }
        public int ItSystemId { get; set; }
        public int ItInterfaceId { get; set; }

        /// <summary>
        /// The local usage of an interface, which this DataRowUsage is bound to.
        /// </summary>
        public virtual ItInterfaceUsage ItInterfaceUsage { get; set; }

        public int DataRowId { get; set; }
        /// <summary>
        /// The DataRow that is in use.
        /// </summary>
        public virtual DataRow DataRow { get; set; }

        public int? FrequencyId { get; set; }
        /// <summary>
        /// How often the data of the DataRow is used
        /// </summary>
        public virtual FrequencyType Frequency { get; set; }

        /// <summary>
        /// How much the data of the DataRow is used
        /// </summary>
        public int? Amount { get; set; }

        /// <summary>
        /// Details regarding total economy of the usage of DataRow
        /// </summary>
        public int? Economy { get; set; }

        /// <summary>
        /// Details regarding the price of this DataRow
        /// </summary>
        public int? Price { get; set; }
    }
}
