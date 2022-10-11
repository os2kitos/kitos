namespace Core.DomainModel.Organization
{
    public enum OrganizationUnitOrigin
    {
        /// <summary>
        /// Organization unit is created and maintained in kitos
        /// </summary>
        Kitos = 0,
        /// <summary>
        /// Organization unit was created and is maintained in STS Organisation
        /// </summary>
        STS_Organisation = 1
    }
}
