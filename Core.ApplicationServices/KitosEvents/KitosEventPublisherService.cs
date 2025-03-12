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
        var dto = _kitosEventMapper.MapKitosEventToDTO(kitosEvent);
        PostEventDTO(dto);
    }

    private void PostEventDTO(KitosEventDTO dto)
    {
        var postMethod = Task.Run(() =>
        {
            try
            {
                _httpEventPublisher.PostEventAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Failed to post events. Exception: {ex}");
                throw;
            }
        });
        postMethod.Wait();
    }
}