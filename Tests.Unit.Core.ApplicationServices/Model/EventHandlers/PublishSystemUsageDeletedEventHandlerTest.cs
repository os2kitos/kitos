using System;
using Core.ApplicationServices.KitosEvents;
using Core.ApplicationServices.Model.KitosEvents;
using Core.DomainModel.Events;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.EventHandlers;

public class PublishSystemUsageDeletedEventHandlerTest : WithAutoFixture
{
    private readonly Mock<IKitosEventPublisherService> _publisher = new();
    private readonly PublishSystemUsageDeletedEventHandler _sut;

    public PublishSystemUsageDeletedEventHandlerTest()
    {
        _sut = new PublishSystemUsageDeletedEventHandler(_publisher.Object);
    }

    [Fact]
    public void Handle_DeletedUsage_PublishesDedicatedTopicAndExactPayload()
    {
        var domainEvent = new SystemTakenOutOfUsageEvent(A<Guid>(), A<Guid>());
        KitosEvent? published = null;
        _publisher.Setup(x => x.PublishEvent(It.IsAny<KitosEvent>()))
            .Callback<KitosEvent>(value => published = value);

        _sut.Handle(domainEvent);

        _publisher.Verify(x => x.PublishEvent(It.IsAny<KitosEvent>()), Times.Once);
        Assert.NotNull(published);
        Assert.Equal("KitosItSystemUsageDeletedEvent", published.Topic);
        var dto = new KitosEventMapper().MapKitosEventToDTO(published);
        var payload = Assert.IsType<System.Collections.Generic.Dictionary<string, object?>>(dto.Payload);
        Assert.Equal(2, payload.Count);
        Assert.Equal(domainEvent.SystemUuid, payload["SystemUuid"]);
        Assert.Equal(domainEvent.OrganizationUuid, payload["OrganizationUuid"]);
    }
}
