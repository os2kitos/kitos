using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
    public class TextController : GenericApiController<Text, TextDTO>
    {
        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
        }
    }
}
