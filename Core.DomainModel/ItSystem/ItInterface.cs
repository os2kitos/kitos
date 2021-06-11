using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Qa.References;
using Infrastructure.Services.Types;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.ItSystem
{
    public class ItInterface : ItSystemBase, IHasRightsHolder, IHasUuid
    {
        public static int MaxNameLength = 100;
        public ItInterface()
        {
            DataRows = new List<DataRow>();
            Uuid = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }
        public string Url { get; set; }
        /// <summary>
        ///     Gets or sets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets the user defined interface identifier.
        /// </summary>
        /// <remarks>
        ///     This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        ///     The user defined interface identifier.
        /// </value>
        public string ItInterfaceId { get; set; }

        public int? InterfaceId { get; set; }

        /// <summary>
        ///     Gets or sets the interface option.
        ///     Provides details about an it system of type interface.
        /// </summary>
        /// <value>
        ///     The interface option.
        /// </value>
        public virtual InterfaceType Interface { get; set; }

        public virtual ICollection<DataRow> DataRows { get; set; }
        public string Note { get; set; }

        /// <summary>
        ///     Gets or sets it system that exhibits this interface instance.
        /// </summary>
        /// <value>
        ///     The it system that exhibits this instance.
        /// </value>
        public virtual ItInterfaceExhibit ExhibitedBy { get; set; }

        public virtual ICollection<BrokenLinkInInterface> BrokenLinkReports { get; set; }

        public bool Disabled { get; set; }

        public virtual ICollection<SystemRelation> AssociatedSystemRelations { get; set; }

        public Maybe<ItInterfaceExhibit> ChangeExhibitingSystem(Maybe<ItSystem> system)
        {
            if (system.IsNone)
            {
                ExhibitedBy = null;
            }
            else
            {
                var newSystem = system.Value;

                var changed =
                    ExhibitedBy == null ||
                    (newSystem.Id != ExhibitedBy.ItSystem.Id);

                if (changed)
                {
                    ExhibitedBy = new ItInterfaceExhibit
                    {
                        ItInterface = this,
                        ItSystem = newSystem
                    };
                }
            }

            return ExhibitedBy;
        }

        public Maybe<int> GetRightsHolderOrganizationId()
        {
            var id = ExhibitedBy?.ItSystem?.BelongsToId ?? ExhibitedBy?.ItSystem?.BelongsTo?.Id;
            return id ?? Maybe<int>.None;
        }
    }
}