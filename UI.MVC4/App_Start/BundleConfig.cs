using System.Web;
using System.Web.Optimization;

namespace UI.MVC4
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery and plugins
            bundles.Add(new ScriptBundle("~/Scripts/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/holder.js",
                "~/Scripts/select2.js"));

            /*
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
             * 
             */

            // standalone libraries
            bundles.Add(new ScriptBundle("~/Scripts/libraries").Include(
                "~/Scripts/underscore.js",
                "~/Scripts/lodash.js",
                "~/Scripts/wysihtml5/wysihtml5-{version}.js"));

            // angularjs and plugins
            bundles.Add(new ScriptBundle("~/Scripts/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-route.js",
                "~/Scripts/AngularUI/ui-router.js",
                "~/Scripts/ui-bootstrap-tpls-{version}.js",
                "~/Scripts/ui-select2.js",
                "~/Scripts/restangular.js",
                "~/Scripts/angular-growl/*.js",
                "~/Scripts/xeditable.js",
                "~/Scripts/angular-ui-util/ui-utils.js"));

            // angular app files
            bundles.Add(new ScriptBundle("~/Scripts/app").IncludeDirectory(
                "~/Scripts/app", "*.js", true));

            // css
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap-cosmo.*",
                "~/Content/carousel.css",
                "~/Content/angular-growl/growl.css",
                "~/Content/css/select2.css",
                "~/Content/css/bootstrap-wysihtml5.css",
                "~/Content/select2-bootstrap.css",
                "~/Content/xeditable.css",
                "~/Content/kitos.css"));
        }
    }
}