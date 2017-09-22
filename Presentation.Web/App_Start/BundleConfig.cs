using System.Web.Optimization;

namespace Presentation.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

            // vendor bundle
            bundles.Add(new ScriptBundle("~/Scripts/vendor")
                .Include("~/Scripts/library-bundle.min.js")
                .Include("~/app/utility/lodash.mixin.js")
            );

            // angular module bundle
            bundles.Add(new ScriptBundle("~/Scripts/angular")
                .Include("~/Scripts/angular-bundle.min.js")
            );
            
            // app bundle
#if DEBUG
            bundles.Add(new ScriptBundle("~/Scripts/app")
                .Include("~/app/app.js")
                .IncludeDirectory("~/app", "*.module.js", true)
                .IncludeDirectory("~/app", "*.js", true)
            );
#else
            bundles.Add(new ScriptBundle("~/Scripts/app")
                .Include("~/Scripts/app-bundle.min.js")
            );
#endif
            // Ignore test specs
            bundles.IgnoreList.Ignore("*.spec.js");
            bundles.IgnoreList.Ignore("*.po.js");

            // styles bundle
            bundles.Add(new StyleBundle("~/Content/css/css")
                .Include("~/Content/css/app.min.css")
            );
        }
    }
}
