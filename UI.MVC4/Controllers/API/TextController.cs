using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class TextController : GenericApiController<Text, string>
    {
        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
        }
    }
}
