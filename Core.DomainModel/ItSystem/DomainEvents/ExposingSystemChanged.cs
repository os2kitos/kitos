using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.ItSystem.DomainEvents
{
    public class ExposingSystemChanged: IDomainEvent
    {
        public ItInterface Interface { get; }
        // TODO: Why do we need the ExposingSystem info when there is an Interface.AssociatedSystemRelations?
        //public Maybe<ItSystem> ExposingSystem { get; }

        public ExposingSystemChanged(ItInterface @interface/*, Maybe<ItSystem> exposingSystem*/)
        {
            Interface = @interface;
            //ExposingSystem = exposingSystem;
        }
    }
}
