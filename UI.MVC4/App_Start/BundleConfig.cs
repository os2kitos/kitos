using System.Web;
using System.Web.Optimization;

namespace UI.MVC4
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/Scripts/select2").Include(
                "~/Scripts/select2.js"
            ));

            /*
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
             * 
             */

            bundles.Add(new ScriptBundle("~/Scripts/libraries")
                .Include("~/Scripts/underscore.js")
                .Include("~/Scripts/lodash.js")
                .Include("~/Scripts/xeditable.js"));

            bundles.Add(new ScriptBundle("~/Scripts/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-route.js",
                "~/Scripts/AngularUI/ui-router.js",
                "~/Scripts/ui-bootstrap-tpls-{version}.js",
                "~/Scripts/ui-select2.js",
                "~/Scripts/restangular.js",
                "~/Scripts/angular-growl/*.js"));

            bundles.Add(new ScriptBundle("~/Scripts/app").IncludeDirectory(
                "~/Scripts/app", "*.js", true));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap-cosmo.*",
                "~/Content/carousel.css",
                "~/Content/angular-growl/growl.css",
                "~/Content/css/select2.css",
                "~/Content/select2-bootstrap.css",
                "~/Content/xeditable.css",
                "~/Content/kitos.css"));
        }
    }
}