﻿using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    public class ItInterface : ItSystemBase
    {
        public ItInterface()
        {
            this.DataRows = new List<DataRow>();
            this.CanBeUsedBy = new List<ItInterfaceUse>();
            this.InterfaceLocalUsages = new List<InterfaceUsage>();
            this.InterfaceLocalExposure = new List<ItInterfaceExhibitUsage>();
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int? VersionOptionId { get; set; }
        public virtual VersionOption VersionOption { get; set; }

        /// <summary>
        /// Gets or sets the user defined interface identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// The user defined interface identifier.
        /// </value>
        public string ItInterfaceId { get; set; }

        public int? InterfaceId { get; set; }
        /// <summary>
        /// Gets or sets the interface option.
        /// Provides details about an it system of type interface.
        /// </summary>
        /// <value>
        /// The interface option.
        /// </value>
        public virtual Interface Interface { get; set; }

        public int? InterfaceTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the interface.
        /// Provides details about an it system of type interface.
        /// </summary>
        /// <value>
        /// The type of the interface.
        /// </value>
        public virtual InterfaceType InterfaceType { get; set; }

        public int? TsaId { get; set; }
        public virtual Tsa Tsa { get; set; }

        public int? MethodId { get; set; }
        public virtual Method Method { get; set; }

        public virtual ICollection<DataRow> DataRows { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Gets or sets it systems that can use this instance.
        /// </summary>
        /// <value>
        /// It systems that can used by this instance.
        /// </value>
        public virtual ICollection<ItInterfaceUse> CanBeUsedBy { get; set; }

        //public int? ExhibitedById { get; set; } TODO
        /// <summary>
        /// Gets or sets it system that exhibits this interface instance.
        /// </summary>
        /// <value>
        /// The it system that exhibits this instance.
        /// </value>
        public virtual ItInterfaceExhibit ExhibitedBy { get; set; }

        /// <summary>
        /// Gets or sets local usages of the system, in case the system is an interface.
        /// </summary>
        /// <value>
        /// The interface local usages.
        /// </value>
        public virtual ICollection<InterfaceUsage> InterfaceLocalUsages { get; set; }

        /// <summary>
        /// Gets or sets local exposure of the system, in case the system is an interface.
        /// </summary>
        /// <value>
        /// The interface local exposure.
        /// </value>
        public virtual ICollection<ItInterfaceExhibitUsage> InterfaceLocalExposure { get; set; }
    }
}