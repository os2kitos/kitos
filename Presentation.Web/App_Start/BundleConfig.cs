using System.Web.Optimization;

namespace Presentation.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            // vendor scripts files
            bundles.Add(new ScriptBundle("~/Scripts/vendor")
                .Include("~/bower_components/lodash/lodash.js")
                .Include("~/bower_components/jquery/dist/jquery.js")
                .Include("~/bower_components/select2/select2.js")
                .Include("~/bower_components/select2/select2_locale_da.js")
                .Include("~/bower_components/moment/moment.js")
                .Include("~/bower_components/bootstrap/dist/js/bootstrap.js")
                .Include("~/bower_components/jsonfn-bower/jsonfn.js")
            );

            // vendor styles
            bundles.Add(new StyleBundle("~/Content/css/css")
                .Include("~/bower_components/bootstrap/dist/css/bootstrap.min.css")
                .Include("~/bower_components/font-awesome/css/font-awesome.min.css")
                .Include("~/bower_components/select2/select2.css")
                .Include("~/bower_components/select2-bootstrap-css/select2-bootstrap.min.css")
                .Include("~/bower_components/angular-loading-bar/build/loading-bar.min.css")
                .Include("~/Content/kitos.css")
                .Include("~/Content/notify/notify.css")
            );

            // angular lib files
            bundles.Add(new ScriptBundle("~/Scripts/angular")
                .Include("~/bower_components/angular/angular.js")
                .Include("~/bower_components/angular-i18n/angular-locale_da-dk.js")
                .Include("~/bower_components/angular-animate/angular-animate.js")
                .Include("~/bower_components/angular-sanitize/angular-sanitize.js")
                .Include("~/bower_components/angular-ui-router/release/angular-ui-router.js")
                .Include("~/bower_components/angular-bootstrap/ui-bootstrap-tpls.js")
                .Include("~/bower_components/angular-ui-select2/src/select2.js")
                .Include("~/bower_components/angular-loading-bar/build/loading-bar.js")
            );

            // angular app files
            bundles.Add(new ScriptBundle("~/Scripts/app")
                .Include("~/app/app.js")
                .IncludeDirectory("~/app", "*.module.js", true)
                .IncludeDirectory("~/app", "*.js", true)
            );

            // Ignore test specs
            bundles.IgnoreList.Ignore("*.spec.js");
            bundles.IgnoreList.Ignore("*.po.js");
        }
    }
}
