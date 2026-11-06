using System;
using Core.ApplicationServices.Model.KitosEvents;
using System.Threading.Tasks;
using Serilog;

namespace Core.ApplicationServices.KitosEvents;

public class KitosEventPublisherService : IKitosEventPublisherService
{
    private readonly IHttpEventPublisher _httpEventPublisher;
    private readonly IKitosEventMapper _kitosEventMapper;
    private readonly ILogger _logger;

    public KitosEventPublisherService(IHttpEventPublisher httpEventPublisher, ILogger logger, IKitosEventMapper kitosEventMapper)
    {
        _httpEventPublisher = httpEventPublisher;
        _logger = logger;
        _kitosEventMapper = kitosEventMapper;
    }
    public void PublishEvent(KitosEvent kitosEvent)
    {
        try
        {
            var dto = _kitosEventMapper.MapKitosEventToDTO(kitosEvent);
            PostEventDTO(dto);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to publish event for topic {Topic}", kitosEvent.Topic);
        }
    }

    private void PostEventDTO(KitosEventDTO dto)
    {
        var postMethod = Task.Run(async () =>
        {
            var result = await _httpEventPublisher.PostEventAsync(dto);
            if (result.Failed)
            {
                _logger.Error("Failed to publish event for topic {Topic}: {Error}", dto.Topic, result.Error);
                return;
            }
            using var response = result.Value;
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to publish event for topic {Topic}: HTTP {StatusCode}", dto.Topic, response.StatusCode);
            }
        });
        postMethod.Wait();
    }
}
