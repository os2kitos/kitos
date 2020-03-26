using System.Linq;

namespace Core.DomainServices.SSO
{
    public class StsPersonData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public StsPersonData(string fullName)
        {
            var nameParts = fullName.Split(' ');
            FirstName = nameParts.First();
            LastName = string.Join(" ", nameParts.Skip(1));
        }
    }
}
