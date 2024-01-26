using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainServices.Organizations;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class StsTokenFetcherTest
    {
        private readonly StandardKernel _kernel;

        public StsTokenFetcherTest()
        {
            _kernel = new StandardKernel();
        }

        [Fact]
        private void Can_Resolve_StsOrganisationTypes()
        {
            _kernel.CanResolve<IStsOrganizationCompanyLookupService>();
            _kernel.CanResolve<IStsOrganizationService>();
            _kernel.CanResolve<IStsOrganizationSystemService>();
        }
    }
}
