using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class GenericCustomOptionApiController<TModel, TReference, TDto> : GenericOptionApiController<TModel, TReference, TDto>
        where TModel : class, ICustomOptionEntity<TReference>
    {
        private readonly IOrganizationService _organizationService;

        public GenericCustomOptionApiController(IGenericRepository<TModel> repository, IOrganizationService organizationService) 
            : base(repository)
        {
            _organizationService = organizationService;
        }

        protected override IEnumerable<TModel> GetAllQuery()
        {
            var orgIds = _organizationService.GetByUser(KitosUser).Select(x => x.Id);
            return this.Repository.Get(t => t.IsActive && !t.IsSuggestion && orgIds.Contains(t.CreatedByOrganizationId.GetValueOrDefault()));
        }

        protected override TModel PostQuery(TModel item)
        {
            var id = KitosUser.DefaultOrganizationUnit.OrganizationId;
            item.CreatedByOrganizationId = id;

            return base.PostQuery(item);
        }
    }
}