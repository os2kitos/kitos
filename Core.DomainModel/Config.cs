using System.Linq;

namespace Core.DomainModel
{
    /// <summary>
    /// Configuration of KITOS for an organization
    /// </summary>
    public class Config : Entity
    {
        /* SHOW/HIDE MODULES */
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        /* IT SUPPORT */
        public int ItSupportModuleNameId { get; set; }
        public string ItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
        public bool ShowColumnMandatory { get; set; }

        /* IT PROJECT */
        public int ItProjectModuleNameId { get; set; }
        public string ItProjectGuide { get; set; }
        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }

        /* IT CONTRACT */
        public int ItContractModuleNameId { get; set; }
        public string ItContractGuide { get; set; }

        public virtual ItSupportModuleName ItSupportModuleName { get; set; }
        public virtual ItProjectModuleName ItProjectModuleName { get; set; }
        public virtual ItContractModuleName ItContractModuleName { get; set; }
        public virtual Organization Organization { get; set; }

        public static Config Default(User objectOwner)
        {
            return new Config()
                {
                    ShowItContractModule = true,
                    ShowItProjectModule = true,
                    ShowItSystemModule = true,
                    ItSupportModuleNameId = 1,
                    ItContractModuleNameId = 1,
                    ItProjectModuleNameId = 1,
                    ItSupportGuide = ".../itunderstøttelsesvejledning",
                    ItProjectGuide = ".../itprojektvejledning",
                    ItContractGuide = ".../itkontraktvejledning",
                    ShowBC = true,
                    ShowPortfolio = true,
                    ShowColumnMandatory = true,
                    ShowColumnTechnology = true,
                    ShowColumnUsage = true,
                    ShowTabOverview = true,
                    ObjectOwner = objectOwner
                };

        }

        /// <summary>
        /// Should only be editable by local admin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (Organization != null && user.AdminRights.Any(right => right.ObjectId == Organization.Id)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
