using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class TextController : GenericApiController<Text, Text>
    {
        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
        }
    }
}
