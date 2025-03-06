namespace Core.ApplicationServices.Model.KitosEvents
{
    public class KitosEventDTO
    {
        public KitosEventDTO(KitosEvent eventSomething)
        {
            Message = eventSomething.EventBody;
            Topic = eventSomething.Topic;
        }
        public object Message { get; }
        public string Topic { get; }

    }
}