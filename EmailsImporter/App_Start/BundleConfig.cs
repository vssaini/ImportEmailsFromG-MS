using System.Web.Optimization;

namespace EmailsImporter
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Enabling Bundling and Minification
            BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/bundles/vendor").Include(
                       "~/Content/vendor/purecounter/purecounter_vanilla.js",
                       "~/Content/vendor/bootstrap/js/bootstrap.bundle.min.js",
                       "~/Content/vendor/glightbox/js/glightbox.min.js",
                       "~/Content/vendor/isotope-layout/isotope.pkgd.min.js",
                       "~/Content/vendor/swiper/swiper-bundle.min.js",
                       "~/Content/vendor/php-email-form/validate.js"
                       ));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //    "~/Content/bootstrap.css",
            //    "~/Content/site.css"));
        }
    }
}
