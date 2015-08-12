using System.Web.Optimization;

namespace Presentation.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
           // standalone libraries
            bundles.Add(new ScriptBundle("~/Scripts/libraries").Include(
                "~/Scripts/underscore.js",
                "~/Scripts/lodash.js",
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/select2.js",
                "~/Scripts/moment.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jsonfn.js"));

            // angularjs and plugins
            bundles.Add(new ScriptBundle("~/Scripts/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/i18n/angular-locale_da-dk.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-sanitize.js",
                "~/Scripts/angular-ui-router.js",
                "~/Scripts/angular-ui/ui-bootstrap.js",
                "~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/Scripts/ui-select2.js",
                "~/Scripts/notify/*.js",
                "~/Scripts/angular-ui-util/ui-utils.js"));

            // angular app files
            bundles.Add(new ScriptBundle("~/Scripts/appbundle").IncludeDirectory(
                "~/Scripts/app", "*.js", true));

            // css
            bundles.Add(new StyleBundle("~/Content/cssbundle").Include(
                "~/Content/bootstrap.css",
                "~/Content/notify/notify.css",
                "~/Content/select2.css",
                "~/Content/select2-bootstrap.css",
                "~/Content/kitos.css"));
        }
    }
}
