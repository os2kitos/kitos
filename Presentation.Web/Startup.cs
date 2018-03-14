using Microsoft.Owin;
using Owin;
using Hangfire;
using IdentityServer3.AccessTokenValidation;
using System.IdentityModel.Tokens;
using Presentation.Web.Infrastructure;
using System.Text;

[assembly: OwinStartup(typeof(Presentation.Web.Startup))]

namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            var ssoConfig = new TokenValidator().GetKeyFromConfig();

            string key = "s7j4HFxlsMJ1HzNt0BMKvuuiTV8BFVTiZgtj3NPkxFnbJ9uzbtTxkYP0waZemiT1wDtggxtcSApCmiPgTl8oAVjFJZTc3xHbN1HPXIAHFDe576uCFpPntezPUYQj2V65n8LdqBhAGSlkHPzulk7YSWUmOb2bkaRODedE45m2t6Tr2PBxaI1cdSx03wviXgDAsUdJDWvfkBG8BZe7982jT9ImdVgi2nZBHv0HNjtyOkBNLxIbiLmASQNXld";

            var test = ssoConfig.SigningKey;
            // Create Security key  using private key above:
            var securityKey = new System.IdentityModel.Tokens.InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Initializing the Hangfire scheduler
            GlobalConfiguration.Configuration.UseSqlServerStorage("kitos_HangfireDB");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            //setup token authentication
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = ssoConfig.Audience,
                    ValidateAudience = false,

                    ValidIssuer = ssoConfig.Issuer,
                    ValidateIssuer = true,

                    IssuerSigningKey = securityKey,
                    ValidateIssuerSigningKey = true,

                    ValidateLifetime = true,
                 
             }
        });


        }
    }
}
