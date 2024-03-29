﻿using Core.DomainModel.Events;
using Infrastructure.Ninject.DomainServices;
using Ninject;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class NinjectDomainEventsAdapterTest
    {
        private const int ExpectedDomainEventId = 1;

        private readonly StandardKernel _kernel;

        public NinjectDomainEventsAdapterTest()
        {
            _kernel = new StandardKernel();
        }

        [Fact]
        private void Raise_GivenRegisteredHandler_ThenHandlerIsCalledOnRaisedEvent()
        {
            // Arrange
            var resultHolder = new ResultHolder();
            _kernel.Bind<IResultHolder>().ToConstant(resultHolder);
            _kernel.Bind<IDomainEventHandler<MyDomainEvent>>().To<MyHandler>().InThreadScope();
            var sut = new NinjectDomainEventHandlerMediator(_kernel);

            // Act
            sut.Raise(new MyDomainEvent { Id = ExpectedDomainEventId });

            // Assert
            Assert.Equal(ExpectedDomainEventId, resultHolder.ResultingValue);
        }

        [Fact]
        private void Raise_GivenMultipleRegisteredHandlersOnSameEvent_ThenAllhandlersAreCalledOnRaisedEvent()
        {
            // Arrange
            _kernel.Bind<IResultHolder>().To<ResultHolder>().InThreadScope();
            var firstHandlerResult = new ResultHolder();
            var secondHandlerResult = new ResultHolder();
            _kernel.Bind<IDomainEventHandler<MyDomainEvent>>().ToConstant(new MyHandler(firstHandlerResult)).InThreadScope();
            _kernel.Bind<IDomainEventHandler<MyDomainEvent>>().ToConstant(new MyHandler(secondHandlerResult)).InThreadScope();
            var sut = new NinjectDomainEventHandlerMediator(_kernel);

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
