using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;

namespace Core.DomainModel.ItSystem.DomainEvents
{
    public class ExposingSystemChanged : IDomainEvent
    {
        public ItInterface AffectedInterface { get; }
        public Maybe<ItSystem> PreviousSystem { get; }
        public Maybe<ItSystem> NewSystem { get; }

        public ExposingSystemChanged(ItInterface affectedInterface, Maybe<ItSystem> previousSystem, Maybe<ItSystem> newSystem)
        {
            AffectedInterface = affectedInterface;
            PreviousSystem = previousSystem;
            NewSystem = newSystem;
        }
    }
}
