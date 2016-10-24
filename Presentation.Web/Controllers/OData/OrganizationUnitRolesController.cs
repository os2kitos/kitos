using Core.DomainServices;
using Core.DomainModel.Organization;
using System.Web.OData.Routing;
using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices;
using System.Linq;
namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitRolesController : BaseEntityController<OrganizationUnitRole>
    {
        private IGenericRepository<OrganizationUnitRole> _repository;
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _repository = repository;
        }

        public override IHttpActionResult Patch(int key, Delta<OrganizationUnitRole> delta)
        {
            var initDelta = delta.GetEntity();

            foreach (var t in delta.GetChangedPropertyNames()) {

                if (t.ToLower() == "priority") {
                    //var testPriorFromDelta = 1;

                    var entity = _repository.GetByKey(key);
                    if (entity.priority != 0) {

                        if (delta.GetEntity().priority > entity.priority)
                        {

                            var entityToBeChanged = _repository.Get().FirstOrDefault(x => x.priority == entity.priority + 1);

                        if (entityToBeChanged != null)
                        {
                            entityToBeChanged.priority = entityToBeChanged.priority - 1;
                            _repository.Update(entityToBeChanged);
                            _repository.Save();
                        }
                        else {
                                if (entity.priority > 0)
                            initDelta.priority = entity.priority;
                        }
                    }
                    else {
                                var entityToBeChanged = _repository.Get().FirstOrDefault(x => x.priority == entity.priority - 1);

                                if (entityToBeChanged != null)
                                {
                                    entityToBeChanged.priority = entityToBeChanged.priority + 1;
                                    _repository.Update(entityToBeChanged);
                                    _repository.Save();
                                }
                                else
                                {
                                    initDelta.priority = entity.priority;
                                }
                        }
                    break;
                }
                    else
                    {
                        if (delta.GetEntity().priority > entity.priority)
                        {
                            var entitiesToBeChanged = _repository.Get(x => x.priority >= initDelta.priority);

                            if (entitiesToBeChanged.Count() > 0)
                            {
                                foreach (var e in entitiesToBeChanged) { 
                                e.priority = e.priority + 1;
                                _repository.Update(e);
                                _repository.Save();
                                }
                            }
                            else
                            {
                                if (entity.priority >= 1) { 
                                initDelta.priority = entity.priority;
                                }
                            }
                        }
                    }
                }
            }
            return base.Patch(key, delta);
        }
    }
}
