using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;

namespace Tools.Test.Database.Model.Extensions
{
    public static class KitosContextExtensions
    {
        public static User GetGlobalAdmin(this KitosContext context)
        {
            return context.Users.First(x => x.IsGlobalAdmin);
        }

        public static Organization GetCommonOrganization(this KitosContext context)
        {
            return context.Organizations.First(x => x.Name == "Fælles Kommune");
        }
    }
}
