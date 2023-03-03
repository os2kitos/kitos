using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Qa.References;


// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.ItSystem
{
    public class ItInterface : ItSystemBase, IHasRightsHolder, IHasUuid, IEntityWithEnabledStatus
    {
        public const int MaxNameLength = 100;
        public const int MaxVersionLength = 20;

        public ItInterface()
        {
            DataRows = new List<DataRow>();
            Uuid = Guid.NewGuid();
            AssociatedSystemRelations = new List<SystemRelation>();
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

        public void Deactivate()
        {
            Disabled = true;
        }

        public IEnumerable<string> UsedByOrganizationNames
        {
            get
            {
                return AssociatedSystemRelations.GroupBy(x => (x.FromSystemUsage.Organization.Id, x.FromSystemUsage.Organization.Name)).Distinct().Select(x => x.Key.Name).OrderBy(x => x).ToList();
            }
        }

        public void ChangeOrganization(Organization.Organization newOrganization)
        {
            if (newOrganization == null)
            {
                throw new ArgumentNullException(nameof(newOrganization));
            }
            Organization = newOrganization;
            OrganizationId = newOrganization.Id;
        }

        public void Activate()
        {
            Disabled = false;
        }

        public DataRow AddDataRow(string dataDescription, Maybe<DataType> dataType)
        {
            var dataRow = new DataRow
            {
                ItInterface = this,
                Data = dataDescription,
                DataType = dataType.GetValueOrDefault()
            };
            DataRows.Add(dataRow);
            return dataRow;
        }

        public Maybe<DataRow> GetDataRow(Guid dataUuid)
        {
            return DataRows.SingleOrDefault(x => x.Uuid == dataUuid).FromNullable();
        }
    }
}