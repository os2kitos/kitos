using Ninject.Modules;

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
