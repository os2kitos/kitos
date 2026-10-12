using System;
using Core.DomainModel.Events;
using Infrastructure.Ninject.DomainServices;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class NinjectDomainEventsAdapterTest
    {
        private const int ExpectedDomainEventId = 1;

        private static IServiceProvider BuildServiceProvider(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);
            return services.BuildServiceProvider();
        }

        [Fact]
        private void Raise_GivenRegisteredHandler_ThenHandlerIsCalledOnRaisedEvent()
        {
            // Arrange
            var resultHolder = new ResultHolder();
            var provider = BuildServiceProvider(s =>
            {
                s.AddSingleton<IResultHolder>(resultHolder);
                s.AddScoped<IDomainEventHandler<MyDomainEvent>, MyHandler>();
            });
            var sut = new NinjectDomainEventHandlerMediator(provider);

            // Act
            sut.Raise(new MyDomainEvent { Id = ExpectedDomainEventId });

            // Assert
            Assert.Equal(ExpectedDomainEventId, resultHolder.ResultingValue);
        }

        [Fact]
        private void Raise_GivenMultipleRegisteredHandlersOnSameEvent_ThenAllhandlersAreCalledOnRaisedEvent()
        {
            // Arrange
            var firstHandlerResult = new ResultHolder();
            var secondHandlerResult = new ResultHolder();
            var provider = BuildServiceProvider(s =>
            {
                s.AddSingleton<IDomainEventHandler<MyDomainEvent>>(new MyHandler(firstHandlerResult));
                s.AddSingleton<IDomainEventHandler<MyDomainEvent>>(new MyHandler(secondHandlerResult));
            });
            var sut = new NinjectDomainEventHandlerMediator(provider);

            // Act
            sut.Raise(new MyDomainEvent { Id = ExpectedDomainEventId });

            // Assert
            Assert.Equal(ExpectedDomainEventId, firstHandlerResult.ResultingValue);
            Assert.Equal(ExpectedDomainEventId, secondHandlerResult.ResultingValue);
        }

        #region Helpers

        public interface IResultHolder
        {
            int ResultingValue { get; set; }
        }

        public class ResultHolder : IResultHolder
        {
            public int ResultingValue { get; set; }
        }

        public class MyDomainEvent : IDomainEvent
        {
            public int Id { get; set; }
        }

        public class MyHandler : IDomainEventHandler<MyDomainEvent>
        {
            private readonly IResultHolder _holder;

            public MyHandler(IResultHolder holder)
            {
                _holder = holder;
            }

            public void Handle(MyDomainEvent domainEvent)
            {
                _holder.ResultingValue = domainEvent.Id;
            }
        }

        #endregion
    }
}
