using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TextController : GenericApiController<Text, TextDTO>
    {
        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
        }
    }
}
