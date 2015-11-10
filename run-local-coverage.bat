echo off
cls

set COVERAGE_FILTER_CORE_APPLICATIONSERVICES=+[Core.ApplicationServices]*
set COVERAGE_FILTER_CORE_DOMAINMODEL=+[Core.DomainModel]*
set COVERAGE_FILTER_CORE_DOMAINSERVICES=+[Core.DomainServices]*
set COVERAGE_FILTER_INFRASTRUCTURE_DATAACCESS=+[Infrastructure.DataAccess]*
set COVERAGE_FILTER_INFRASTRUCTURE_OPENXML=+[Infrastructure.OpenXML]*
set COVERAGE_FILTER_PRESENTATION_WEB=+[Presentation.Web]*
set COVERAGE_EXCL_FILTER_PRESENTATION_WEB=-[Presentation.Web]Presentation.Web.App_Start* -[Presentation.Web]Presentation.Web.AuthConfig -[Presentation.Web]Presentation.Web.BundleConfig -[Presentation.Web]Presentation.Web.DropdownProfile -[Presentation.Web]Presentation.Web.FilterConfig -[Presentation.Web]Presentation.Web.Infrastructure.NinjectDependencyResolver -[Presentation.Web]Presentation.Web.Infrastructure.NinjectDependencyScope -[Presentation.Web]Presentation.Web.Infrastructure.UserRepositoryFactory -[Presentation.Web]Presentation.Web.Infrastructure.ProviderInitializationHttpModule -[Presentation.Web]Presentation.Web.MappingProfile -[Presentation.Web]Presentation.Web.MappingConfig -[Presentation.Web]Presentation.Web.Models* -[Presentation.Web]Presentation.Web.MvcApplication -[Presentation.Web]Presentation.Web.Properties.Settings -[Presentation.Web]Presentation.Web.RouteConfig -[Presentation.Web]Presentation.Web.Startup -[Presentation.Web]Presentation.Web.WebApiConfig
set TEST_ASSEMBLIES=Tests.Unit.Presentation.Web\bin\Debug\Tests.Unit.Presentation.Web.dll

packages\OpenCover.4.6.166\tools\OpenCover.Console.exe -target:run-tests.bat -register:user -filter:"%COVERAGE_FILTER_CORE_APPLICATIONSERVICES% %COVERAGE_FILTER_CORE_DOMAINMODEL% %COVERAGE_FILTER_CORE_DOMAINSERVICES% %COVERAGE_FILTER_INFRASTRUCTURE_DATAACCESS% %COVERAGE_FILTER_INFRASTRUCTURE_OPENXML% %COVERAGE_FILTER_PRESENTATION_WEB% %COVERAGE_EXCL_FILTER_PRESENTATION_WEB%"
packages\ReportGenerator.2.3.2.0\tools\ReportGenerator.exe -reports:results.xml -targetdir:coverage

start .\coverage\index.htm
