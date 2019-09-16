namespace Core.DomainServices.Authorization
{
    public class DataAccessLevel
    {
        public CrossOrganizationDataReadAccessLevel CrossOrganizational { get; }
        public OrganizationDataReadAccessLevel CurrentOrganization { get; }

        public DataAccessLevel(CrossOrganizationDataReadAccessLevel crossOrganizational, OrganizationDataReadAccessLevel currentOrganization)
        {
            CrossOrganizational = crossOrganizational;
            CurrentOrganization = currentOrganization;
        }

        protected bool Equals(DataAccessLevel other)
        {
            return CrossOrganizational == other.CrossOrganizational && CurrentOrganization == other.CurrentOrganization;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataAccessLevel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) CrossOrganizational * 397) ^ (int) CurrentOrganization;
            }
        }
    }
}
