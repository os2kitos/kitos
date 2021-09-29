﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Newtonsoft.Json;
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

        public class RootClass
        {
            public string R1 { get; set; }
            public ChildClass Child { get; set; }
        }
    }

    public class CurrentAspNetRequestTest : WithAutoFixture, IDisposable
    {
        private readonly MemoryStream _testStream;
        private readonly CurrentAspNetRequest _sut;

        public CurrentAspNetRequestTest()
        {
            _testStream = new MemoryStream();
            var requestStreamMock = new Mock<ICurrentRequestStream>();
            requestStreamMock.Setup(x => x.GetCurrentInputStream()).Returns(_testStream);
            _sut = new CurrentAspNetRequest(Mock.Of<ILogger>(), requestStreamMock.Object);
        }

        [Fact]
        public void Can_Get_All_Properties_At_Root()
        {
            //Arrange
            var rootClass = A<ChildClass.RootClass>();
            SetStreamContent(rootClass);

            //Act
            var definedJsonProperties = _sut.GetDefinedJsonProperties();

            //Assert
            Assert.Equal(GetAllPropertyNames<ChildClass.RootClass>().OrderBy(x => x), definedJsonProperties.OrderBy(x => x));
        }

        private void SetStreamContent(ChildClass.RootClass rootClass)
        {
            var json = JsonConvert.SerializeObject(rootClass);
            var writer = new StreamWriter(_testStream);
            writer.Write(json);
            writer.Flush();
            _testStream.Position = 0;
        }
        //TODO: PArtials
        //TODO: Missing sectoins - empty result

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
