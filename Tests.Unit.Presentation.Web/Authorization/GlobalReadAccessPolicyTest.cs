using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization.Policies;
using Core.DomainModel;
using Fasterflect;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class GlobalReadAccessPolicyTest
    {
        private readonly GlobalReadAccessPolicy _sut;

        public GlobalReadAccessPolicyTest()
        {
            _sut = new GlobalReadAccessPolicy();
        }

        [Fact]
        public void Allow_Returns_False_For_All_Unknown_Entity_Types()
        {
            //Arrange
            var expectedReadAccess = GetKnownTypes();
            var nonGlobalReadEntityTypes =
                typeof(Entity)
                    .Assembly
                    .GetTypes()
                    .Where(x => typeof(IEntity).IsAssignableFrom(x))
                    .Where(x=>x.IsAbstract == false)
                    .Where(x=>x.IsInterface == false)
                    .Where(x => expectedReadAccess.ContainsKey(x) == false)
                    .ToList();

            foreach (var type in nonGlobalReadEntityTypes)
            {
                var entity = (IEntity)Activator.CreateInstance(type);
                
                //Act
                var allow = _sut.Allow(entity);
                
                //Assert
                Assert.False(allow);
            }
        }

        [Fact]
        public void Allow_Returns_True_For_All_Known_Entity_Types()
        {
            //Arrange
            var expectedReadAccess = GetKnownTypes();

            foreach (var type in expectedReadAccess.Keys)
            {
                var entity = (IEntity)Activator.CreateInstance(type);

                //Act
                var allow = _sut.Allow(entity);

                //Assert
                Assert.True(allow);
            }
        }

        private IReadOnlyDictionary<Type, bool> GetKnownTypes()
        {
            return (IReadOnlyDictionary<Type, bool>)_sut.GetFieldValue("TypesWithGlobalReadAccess");
        }
    }
}
