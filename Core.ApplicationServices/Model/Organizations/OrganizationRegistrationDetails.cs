namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationDetails
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public OrganizationRegistrationType Type { get; set; }
        public int? ObjectId { get; set; }
        public string ObjectName { get; set; }
        public int? PaymentIndex { get; set; }
    }
}
