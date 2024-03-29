﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Abstractions.Extensions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Model.Request;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class GrandChildClass
    {
        public string GC1 { get; set; }
        public string GC2 { get; set; }
    }

    public class ChildClass
    {
        public string C1 { get; set; }
        public string C2 { get; set; }
        public GrandChildClass GrandChild { get; set; }
    }

    public class RootClass
    {
        public string R1 { get; set; }
        public ChildClass Child { get; set; }
    }

    public class CurrentAspNetRequestTest : WithAutoFixture, IDisposable
    {
        private readonly MemoryStream _testStream;
        private readonly CurrentAspNetRequest _sut;

        public CurrentAspNetRequestTest()
        {
            _testStream = new MemoryStream();
            var requestStreamMock = new Mock<ICurrentRequestStream>();
            requestStreamMock.Setup(x => x.GetInputStreamCopy()).Returns(_testStream);
            _sut = new CurrentAspNetRequest(Mock.Of<ILogger>(), requestStreamMock.Object);
        }

        [Fact]
        public void Can_Get_All_Properties_At_Root()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var definedJsonProperties = _sut.GetDefinedJsonProperties(Enumerable.Empty<string>());

            //Assert
            Assert.Equal(GetAllPropertyNames<RootClass>().OrderBy(x => x), definedJsonProperties.OrderBy(x => x));
        }

        [Fact]
        public void Can_Get_All_Properties_At_Child()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var definedJsonProperties = _sut.GetDefinedJsonProperties(nameof(RootClass.Child).WrapAsEnumerable());

            //Assert
            Assert.Equal(GetAllPropertyNames<ChildClass>().OrderBy(x => x), definedJsonProperties.OrderBy(x => x));
        }

        [Fact]
        public void Can_Get_All_Properties_At_GrandChild()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var definedJsonProperties = _sut.GetDefinedJsonProperties(new[] { nameof(RootClass.Child), nameof(RootClass.Child.GrandChild) });

            //Assert
            Assert.Equal(GetAllPropertyNames<GrandChildClass>().OrderBy(x => x), definedJsonProperties.OrderBy(x => x));
        }

        [Fact]
        public void Can_Get_Object_If_Set_To_Null()
        {
            //Arrange
            var rootClass = A<RootClass>();
            rootClass.Child = null;
            SetStreamContent(rootClass);

            //Act
            var objectType = _sut.GetObject(nameof(RootClass.Child).WrapAsEnumerable());

            //Assert
            Assert.True(objectType.HasValue);
            Assert.Equal(JTokenType.Null, objectType.Value.Type);
        }

        [Fact]
        public void Can_Get_Object()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var objectType = _sut.GetObject(nameof(RootClass.Child).WrapAsEnumerable());

            //Assert
            Assert.True(objectType.HasValue);
            Assert.Equal(JTokenType.Object, objectType.Value.Type);
        }

        [Fact]
        public void Cannot_Get_Unknown_Object()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var objectType = _sut.GetObject(new[] { nameof(RootClass.Child), nameof(RootClass) });

            //Assert
            Assert.True(objectType.IsNone);
        }

        [Fact]
        public void Returns_Empty_Set_For_NonExistingPath()
        {
            //Arrange
            var rootClass = A<RootClass>();
            SetStreamContent(rootClass);

            //Act
            var definedJsonProperties = _sut.GetDefinedJsonProperties(new[] { nameof(RootClass.Child), nameof(RootClass.Child.GrandChild), nameof(RootClass) });

            //Assert
            Assert.Empty(definedJsonProperties);
        }

        private void SetStreamContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var writer = new StreamWriter(_testStream);
            writer.Write(json);
            writer.Flush();
            _testStream.Position = 0;
        }

        protected static HashSet<string> GetAllPropertyNames<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToHashSet();
        }

        public void Dispose()
        {
            _testStream?.Dispose();
        }
    }
}
