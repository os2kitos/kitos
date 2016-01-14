using System.Web.Optimization;

namespace Presentation.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            // angular app files
            bundles.Add(new ScriptBundle("~/Scripts/app")
                .Include("~/app/app.js")
                .IncludeDirectory("~/app", "*.js", true));

            // Ignore test specs
            bundles.IgnoreList.Ignore("*.spec.js");
            bundles.IgnoreList.Ignore("*.po.js");
        }
    }
}
