using System;
using Core.ApplicationServices.KitosEvents;
using Core.ApplicationServices.Model.KitosEvents;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystemUsage;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.EventHandlers;

public class PublishSystemTakenIntoUsageEventHandlerTest : WithAutoFixture
{
    private readonly Mock<IKitosEventPublisherService> _eventPublisher;
    private readonly PublishSystemTakenIntoUsageEventHandler _sut;

    public PublishSystemTakenIntoUsageEventHandlerTest()
    {
        _eventPublisher = new Mock<IKitosEventPublisherService>();
        _sut = new PublishSystemTakenIntoUsageEventHandler(_eventPublisher.Object);
    }

    [Fact]
    public void Can_Publish_System_Taken_Into_Usage()
    {
        var systemUuid = A<Guid>();
        var organizationUuid = A<Guid>();
        var systemUsage = new ItSystemUsage();

        _sut.Handle(new SystemTakenIntoUsageEvent(systemUsage, systemUuid, organizationUuid));

        _eventPublisher.Verify(x => x.PublishEvent(It.Is<KitosEvent>(e =>
            e.Topic == KitosQueueTopics.KitosItSystemUsageCreatedEvent &&
            ((SystemTakenIntoUsageEventBodyModel)e.EventBody).SystemUuid == systemUuid &&
            ((SystemTakenIntoUsageEventBodyModel)e.EventBody).OrganizationUuid == organizationUuid)), Times.Once);
    }
}
