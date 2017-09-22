using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices
{
    public class ApplicationServiceModule: NinjectModule
    {
        public override void Load()
        {
            this.Bind<IAdviceService>().To<AdviceService>();
        }
    }
}
