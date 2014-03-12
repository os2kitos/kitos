using System.Web;
using System.Web.Optimization;

namespace UI.MVC4
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            /*bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js"));

            bundles.Add(new ScriptBundle("~/bundles/wysihtml5").Include(
                "~/Scripts/wysihtml5/parser_rules/advanced.js",
                "~/Scripts/wysihtml5/wysihtml5-{version}.js",
                "~/Scripts/bootstrap3-wysihtml5.js"));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                        "~/Scripts/select2.js"));

            bundles.Add(new ScriptBundle("~/bundles/pnotify").Include(
                        "~/Scripts/pnotify/jquery.pnotify.js"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Scripts/form.control.js"));

             * 
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
             * 
             */

            bundles.Add(new ScriptBundle("~/Scripts/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-route.js",
                "~/Scripts/AngularUI/ui-router.js"));

            bundles.Add(new ScriptBundle("~/Scripts/holder").Include("~/Scripts/holder.js"));

            bundles.Add(new ScriptBundle("~/Scripts/app").IncludeDirectory(
                "~/Scripts/app", "*.js", true));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap-cosmo.*", 
                "~/Content/carousel.css",
                //"~/Content/bootstrap3-wysihtml5/bootstrap3-wysihtml5.css",
                //"~/Content/pnotify/*.css",
                //"~/Content/css/select2.css",
                //"~/Content/select2-bootstrap.css",
                "~/Content/kitos.css"));
        }
    }
}