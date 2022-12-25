using System.Web;
using System.Web.Optimization;

namespace EmailsImporter
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/vendor").Include(
                       "~/Content/vendor/purecounter/purecounter_vanilla.js",
                       "~/Content/vendor/bootstrap/js/bootstrap.bundle.min.js",
                       "~/Content/vendor/glightbox/js/glightbox.min.js",
                       "~/Content/vendor/isotope-layout/isotope.pkgd.min.js",
                       "~/Content/vendor/swiper/swiper-bundle.min.js",
                       "~/Content/vendor/php-email-form/validate.js"
                       ));

            //<link href="~/Content/vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet">
            //<link href="~/Content/vendor/bootstrap-icons/bootstrap-icons.css" rel="stylesheet">
            //<link href="~/Content/vendor/boxicons/css/boxicons.min.css" rel="stylesheet">
            //<link href="~/Content/vendor/glightbox/css/glightbox.min.css" rel="stylesheet">
            //<link href="~/Content/vendor/swiper/swiper-bundle.min.css" rel="stylesheet">

            // TODO: Create bundle of above CSS files
            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //    "~/Content/bootstrap.css",
            //    "~/Content/site.css"));
        }
    }
}
