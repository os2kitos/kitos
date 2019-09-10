using System;
using Core.DomainServices.Authorization;

namespace Core.DomainServices.Model
{
    /// <summary>
    /// Defines common parameters for queries for data owned by an organization
    /// </summary>
    public class OrganizationDataQueryParameters
    {
        public int ActiveOrganizationId { get; }
        public OrganizationDataQueryBreadth Breadth { get; }
        public DataAccessLevel DataAccessLevel { get; }

        public OrganizationDataQueryParameters(
            int activeOrganizationId,
            OrganizationDataQueryBreadth breadth,
            DataAccessLevel dataAccessLevel)
        {
            ActiveOrganizationId = activeOrganizationId;
            Breadth = breadth;
            DataAccessLevel = dataAccessLevel ?? throw new ArgumentNullException(nameof(dataAccessLevel));
        }
    }
}
