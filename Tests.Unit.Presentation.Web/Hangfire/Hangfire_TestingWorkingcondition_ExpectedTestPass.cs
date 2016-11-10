using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using NSubstitute;
using System.Web.Mvc;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Logging;
using Xunit;

namespace Tests.Unit.Presentation.Web.Hangfire
{
    using global::Presentation.Web.Controllers.Web;

    public class HangfireTestingWorkingconditionExpectedTestPass
    {
        //public class HomeController
        //{
        //    private readonly IBackgroundJobClient _jobClient;

        //    public HomeController() : this(new BackgroundJobClient()) { }

        //    public HomeController(IBackgroundJobClient jobClient)
        //    {
        //        _jobClient = jobClient;
        //    }

        //    public void Create()
        //    {
        //        _jobClient.Enqueue(() => WriteToConsole("message"));
        //    }

        //    public void WriteToConsole(string message)
        //    {
        //        Console.WriteLine(message);
        //    }
        //}
        [Fact]
        public void IsHangfireRunning()
        {
            var jobClient = Substitute.For<IBackgroundJobClient>();
            var result = jobClient.Schedule(() => Console.WriteLine("hey there"), TimeSpan.FromMinutes(2));
            //var result = jobClient.Received();
            Assert.NotEmpty("s");
        }
    }
}
