namespace Core.DomainModel
{
    public enum OrganizationCategory
    {
        /// <summary>
        /// Is not part of the data sharing between organizations,
        /// and can only work with own objects.
        /// </summary>
        Other = 0,
        /// <summary>
        /// Is part of the data sharing between organizations.
        /// </summary>
        Municipality = 1
    }
}
