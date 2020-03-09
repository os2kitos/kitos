using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Model.ExternalLinks;
using Ninject;

namespace Core.BackgroundJobs.Factory
{
    public class BackgroundJobFactory : IBackgroundJobFactory
    {
        private readonly IKernel _kernel;

        public BackgroundJobFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IAsyncBackgroundJob CreateExternalReferenceCheck()
        {
            return _kernel.Get<CheckExternalLinks>();
        }
    }
}
