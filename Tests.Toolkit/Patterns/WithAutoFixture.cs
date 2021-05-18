using System.Collections.Generic;
using AutoFixture;

namespace Tests.Toolkit.Patterns
{
    public abstract class WithAutoFixture
    {
        private readonly Fixture _fixture;

        protected WithAutoFixture()
        {
            _fixture = new Fixture();
            OnFixtureCreated(_fixture);
        }

        protected virtual void OnFixtureCreated(Fixture fixture)
        {
            //Override to configure fixture-specific defaults
        }

        protected T A<T>()
        {
            return _fixture.Create<T>();
        }

        protected T Freeze<T>()
        {
            return _fixture.Freeze<T>();
        }

        protected IEnumerable<T> Many<T>(int? howMany = null)
        {
            return howMany.HasValue ? _fixture.CreateMany<T>(howMany.Value) : _fixture.CreateMany<T>();
        }
    }
}
