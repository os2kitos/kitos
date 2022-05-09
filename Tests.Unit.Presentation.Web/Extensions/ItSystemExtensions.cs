using System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class ItSystemExtensions
    {
        public static ItSystem WithInterfaceExhibit(this ItSystem system, ItInterface itInterface)
        {
            itInterface.ExhibitedBy(system);
            return system;
        }

        public static ItSystem WithParentSystem(this ItSystem system, ItSystem parentItSystem)
        {
            if (system == parentItSystem)
                throw new ArgumentException($"{nameof(system)} must not be the same as {nameof(parentItSystem)}");

            system.Parent = parentItSystem;
            system.ParentId = parentItSystem?.ParentId;
            parentItSystem?.Children.Add(system);

            return system;
        }

        public static ItSystem WithRightsHolder(this ItSystem system, Organization rightsHolder)
        {
            system.UpdateRightsHolder(rightsHolder);
            rightsHolder.BelongingSystems.Add(system);

            return system;
        }
    }
}
