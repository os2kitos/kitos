using System.Collections.Specialized;
using Infrastructure.Services.DomainEvents;
using Ninject;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class DomainEventsTest
    {
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

            public void Handle(MyDomainEvent args)
            {
                _holder.ResultingValue = args.Id;
            }
        }

        [Fact]
        private void Raise_GivenRegisteredDomainEvent_ThenHandlerIsCalled()
        {
            // Arrange
            var kernel = new StandardKernel();
            var resultHolder = new ResultHolder();
            kernel.Bind<IResultHolder>().ToConstant(resultHolder);
            kernel.Bind<IDomainEventHandler<MyDomainEvent>>().To<MyHandler>().InThreadScope();
            var sut = new DomainEvents(kernel);

            // Act
            const int expectedDomainEventId = 1;
            sut.Raise(new MyDomainEvent { Id = expectedDomainEventId });

            // Assert
            Assert.Equal(expectedDomainEventId, resultHolder.ResultingValue);
        }

        [Fact]
        private void Raise_GivenMultipleRegisteredHandlersOnSameEvent_ThenAllhandlersAreCalled()
        {
            // Arrange
            var kernel = new StandardKernel();
            kernel.Bind<IResultHolder>().To<ResultHolder>().InThreadScope();
            var firstHandlerResult = new ResultHolder();
            var secondHandlerResult = new ResultHolder();
            kernel.Bind<IDomainEventHandler<MyDomainEvent>>().ToConstant(new MyHandler(firstHandlerResult)).InThreadScope();
            kernel.Bind<IDomainEventHandler<MyDomainEvent>>().ToConstant(new MyHandler(secondHandlerResult)).InThreadScope();
            var sut = new DomainEvents(kernel);

            // Act
            const int expectedDomainEventId = 1;
            sut.Raise(new MyDomainEvent { Id = expectedDomainEventId });

            // Assert
            Assert.Equal(expectedDomainEventId, firstHandlerResult.ResultingValue);
            Assert.Equal(expectedDomainEventId, secondHandlerResult.ResultingValue);
        }
    }
}
