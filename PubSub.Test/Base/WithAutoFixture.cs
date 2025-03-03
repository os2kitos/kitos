using AutoFixture;

namespace PubSub.Test.Base
{
    namespace Tests.Toolkit.Patterns
    {
        public abstract class WithAutoFixture
        {
            private readonly Fixture _fixture;

            protected WithAutoFixture()
            {
                _fixture = new Fixture();
            }

            protected T A<T>()
            {
                return _fixture.Create<T>();
            }

            protected IEnumerable<T> Many<T>(int? howMany = null)
            {
                return (howMany.HasValue ? _fixture.CreateMany<T>(howMany.Value) : _fixture.CreateMany<T>()).ToList();
            }
        }
    }

}
