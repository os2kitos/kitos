using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class LocaleController: GenericApiController<Localization, string>
    {
        public LocaleController(IGenericRepository<Localization> repository) 
            : base(repository)
        {
        }
    }
}