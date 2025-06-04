using AutoFixture;
using Presentation.Web.Models.API.V1;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ObjectCreateHelper
    {
        private static readonly Fixture Fixture = new Fixture();

        public static LoginDTO MakeSimpleLoginDto(string email, string pwd)
        {
            return new()
            {
                Email = email,
                Password = pwd
            };
        }
    }
}
