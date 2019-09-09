using System.Web;
using System.Web.Optimization;

namespace TrebovanjaNarudzbe.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            "~/Scripts/jquery-ui-{version}.js"));


            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/EcoOne").Include(
                         "~/Scripts/EcoOne/core.min.js",
                         "~/Scripts/EcoOne/nicescroll.min.js",
                         "~/Scripts/EcoOne/moment.min.js",
                         //"~/Scripts/EcoOne/daterangepicker.js",
                         "~/Scripts/EcoOne/app.min.js"
                       //"~/Scripts/EcoOne/datepicker.js"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/Toastr").Include(
                         "~/Scripts/Toastr/toastr.min.js"
                       ));

            bundles.Add(new ScriptBundle("~/bundles/Select2").Include(
                       "~/Scripts/Select2/select2.full.min.js"
                       ));

            bundles.Add(new StyleBundle("~/Content/Select2").Include(
                       "~/Content/Select2/select2.min.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                 "~/Content/bootstrap.css",
                "~/Content/PagedList.css",
                   "~/Content/ecoone.css",
                   "~/Content/loader.css",
                   "~/Content/Site.css",                   
                   "~/Content/StyleSheet1.css"
                 

                  ));

            bundles.Add(new StyleBundle("~/Content/MyStyle").Include(
                  "~/Content/MyStyle.css"
                 ));

            bundles.Add(new StyleBundle("~/Content/Toastr").Include(
                "~/Content/Toastr/toastr.min.css"
                 ));

            bundles.Add(new StyleBundle("~/Content/DataTable").Include(
                "~/Content/DataTables/datatables.min.css",
                "~/Content/DataTables/buttons.dataTables.min.css"
                ));

          
                  bundles.Add(new ScriptBundle("~/bundles/DataTables").Include(
                  "~/Scripts/DataTables/Moment.js",
                 "~/Scripts/DataTables/datatables.min.js",

                 //scripte za export podataka iz dataTable (pdf...)
                 "~/Scripts/DataTables/dataTables.buttons.min.js",
                 "~/Scripts/DataTables/buttons.flash.min.js",
                 "~/Scripts/DataTables/jszip.min.js",
                 "~/Scripts/DataTables/pdfmake.min.js",
                 "~/Scripts/DataTables/vfs_fonts.js",
                 "~/Scripts/DataTables/html5.min.js",
                 "~/Scripts/DataTables/print.min.js"
           ));

        }
    }
}
