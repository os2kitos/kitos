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
            return context
                .Users
                .AsQueryable()
                .Where(x => x.IsGlobalAdmin)
                .OrderByDescending(x => x.Id)
                .First();
        }

        public static Organization GetCommonOrganization(this KitosContext context)
        {
            return context
                .Organizations
                .First(x => x.Name == "Fælles Kommune");
        }
    }
}
