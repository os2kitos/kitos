using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceTypeController : GenericOptionApiController<InterfaceType, ItSystem, OptionDTO>
    {
        public InterfaceTypeController(IGenericRepository<InterfaceType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<InterfaceType> GetAllQuery(int skip, int take, bool descending, string orderBy = null)
        {
            var field = orderBy ?? "Id";
            return Repository.AsQueryable().Where(x => x.IsActive).OrderByField(field, descending).Skip(skip).Take(take);
        }
    }
}