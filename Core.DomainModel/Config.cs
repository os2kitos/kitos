using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Configuration of KITOS for an organization
    /// </summary>
    public class Config : Entity, IIsPartOfOrganization
    {
        /* SHOW/HIDE MODULES */
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }
        public bool ShowDataProcessing { get; set; }

        /* SHOW/HIDE 'IT' PREFIX */
        public bool ShowItSystemPrefix { get; set; }
        public bool ShowItContractPrefix { get; set; }

        /* IT SUPPORT */
        public int ItSupportModuleNameId { get; set; }
        public string ItSupportGuide { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public static Config Default(User objectOwner)
        {
            return new Config
            {
                    ShowItContractModule = true,
                    ShowItSystemModule = true,
                    ShowItContractPrefix = true,
                    ShowItSystemPrefix = true,
                    ShowDataProcessing = true,
                    ObjectOwner = objectOwner,
                    LastChangedByUser = objectOwner
                };
        }

        public IEnumerable<int> GetOrganizationIds()
        {
            yield return Id; //Same as org id due to how it is bound
        }
    }
}
