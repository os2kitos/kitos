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

        protected bool Equals(OrganizationDataQueryParameters other)
        {
            return ActiveOrganizationId == other.ActiveOrganizationId && Breadth == other.Breadth && Equals(DataAccessLevel, other.DataAccessLevel);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OrganizationDataQueryParameters) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ActiveOrganizationId;
                hashCode = (hashCode * 397) ^ (int) Breadth;
                hashCode = (hashCode * 397) ^ (DataAccessLevel != null ? DataAccessLevel.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
