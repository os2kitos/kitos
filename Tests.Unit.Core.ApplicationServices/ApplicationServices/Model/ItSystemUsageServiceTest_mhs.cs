using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Model
{
    public class ItSystemUsageServiceTest
    {
        [Fact]
        private void ReplaceSystemRelation()
        {
            var mockSystemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            var mockAuthorizationContext = new Mock<IAuthorizationContext>();
            var mockSystemRepository = new Mock<ItSystemRepository>();
            var mockContractRepository = new Mock<ItContractRepository>();
            var mockOptionsService = new Mock<IOptionsService<SystemRelation,RelationFrequencyType>>();
            var mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();

            var sut = new ItSystemUsageService(
                mockSystemUsageRepository.Object, 
                mockAuthorizationContext.Object, 
                mockSystemRepository.Object, 
                mockContractRepository.Object, 
                mockOptionsService.Object, 
                mockOrganizationalUserContext.Object);

            sut.ModifyRelation(int )
        }
    }
}
