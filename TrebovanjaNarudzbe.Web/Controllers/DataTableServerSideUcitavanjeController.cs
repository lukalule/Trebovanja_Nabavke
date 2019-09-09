using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrebovanjaNarudzbe.Web.ViewModels;
using System.Linq.Dynamic;
using TrebovanjaNarudzbe.Models.Models;



namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class DataTableServerSideUcitavanjeController : Controller
    {
        // GET: DataTableServerSideUcitavanje
        string adresaHosta = @"http://localhost:50181/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;

        public DataTableServerSideUcitavanjeController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }


        #region DataTable klase
        //Start - JSon class sent from Datatables


        public class DataTableAjaxPostModel
        {
            // properties are not capital due to json mapping
            public int draw { get; set; }
            public int start { get; set; }
            public int length { get; set; }
            public List<Column> columns { get; set; }
            public Search search { get; set; }
            public List<Order> order { get; set; }
        }

        public class Column
        {
            public string data { get; set; }
            public string name { get; set; }
            public bool searchable { get; set; }
            public bool orderable { get; set; }
            public Search search { get; set; }
        }

        public class Search
        {
            public string value { get; set; }
            public string regex { get; set; }
        }

        public class Order
        {
            public int column { get; set; }
            public string dir { get; set; }
        }
        /// End- JSon class sent from Datatables
        #endregion


        public ActionResult Index()
        {
            return View();
        }
        #region DataTablesFunkcijeZaServerSide
        //pravimo view za ovu f-ju za koji ptavimo DataTables tabelu 
        public ActionResult TabelarniPrikazTrebovanja()
        {
            return View();
        }

        public string SifraLogovanogRadnika()
        {
            string sifraRadnika = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name).RadnikSifra;
            return sifraRadnika;
        }
        //Ucitavanje podataka za DataTables
        public List<TrebovanjeViewModel> ListaTrebovanjaZaDT(string sifraRadnika)
        {
            List<TrebovanjeViewModel> listaTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(t =>
           (t.vRadniks.Nivo1OdobravanjaSifra == sifraRadnika || t.vRadniks.Nivo2OdobravanjaSifra == sifraRadnika || t.vRadniks.Nivo3OdobravanjaSifra == sifraRadnika)
           ).Select(t => new TrebovanjeViewModel
           {
               TrebovanjeId = t.TrebovanjeId,
               SerijskiBroj = t.SerijskiBroj,
               ListaArtikalaTrebovanja = trebovanjeNabavkeContext.TrebovanjeVeznas.Where(tv => tv.TrebovanjeId == t.TrebovanjeId).
                    Select(tv => new ArtiklViewModel()
                    {
                        ArtiklId = tv.ArtikalId,
                        TrebovanaKolicina = tv.TrebovanaKolicina,
                        Naziv = trebovanjeNabavkeContext.vInformacijeOArtiklus.FirstOrDefault(a => a.ArtikalId == tv.ArtikalId).Naziv,
                        Spremno = tv.StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje || tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                        Preuzeto = tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                    }).OrderBy(o => o.Naziv).ToList(),
               SifraRadnika = t.SifraRadnika



           }).ToList();

            return listaTrebovanja;
        }
        //f-ja koju pozivamo preko ajax-a
        public JsonResult ModelZaViewDT(DataTableAjaxPostModel model)
        {
            // action inside a standard controller
            int filteredResultsCount;
            int totalResultsCount;
            var res = PretragaDT(model, out filteredResultsCount, out totalResultsCount);

            var result = new List<TrebovanjeViewModel>(res.Count);
            foreach (var s in res)
            {
                // simple remapping adding extra info to found dataset
                result.Add(new TrebovanjeViewModel
                {
                    TrebovanjeId = s.TrebovanjeId,
                    SerijskiBroj = s.SerijskiBroj,
                    SifraRadnika = s.SifraRadnika,
                    ListaArtikalaTrebovanja = s.ListaArtikalaTrebovanja

                });
            };

            return Json(new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
        public IList<TrebovanjeViewModel> PretragaDT(DataTableAjaxPostModel model, out int filteredResultsCount, out int totalResultsCount)
        {
            var searchBy = (model.search != null) ? model.search.value : null;

            var take = model.length;
            var skip = model.start;

            string sortBy = "";
            bool sortDir = true;

            if (model.order != null)
            {
                // in this example we just default sort on the 1st column
                sortBy = model.columns[model.order[0].column].data;
                sortDir = model.order[0].dir.ToLower() == "asc";
            }

            //if (model.order[0].column != 0)
            //{
            //    // in this example we just default sort on the 1st column
            //    sortBy = model.columns[model.order[0].column].data;
            //    //sortDir = model.order[0].dir.ToLower() == "asc";
            //}


            // search the dbase taking into consideration table sorting and paging
            var result = PodaciIzBazeDT(searchBy, take, skip, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
            if (result == null)
            {
                // empty collection...
                return new List<TrebovanjeViewModel>();
            }
            return result;
        }
        public List<TrebovanjeViewModel> PodaciIzBazeDT(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            // the example datatable used is not supporting multi column ordering
            // so we only need get the column order from the first column passed to us.        
            //var whereClause = BuildDynamicWhereClause(Db, searchBy);


            if (String.IsNullOrEmpty(searchBy))
            {
                // if we have an empty search then just order the results by Id ascending
                //sortBy = "Id";
                sortBy = "TrebovanjeId";
                sortDir = true;
            }

            //Kupljenje šifre ulogovanog radnika

            string sifraRadnika = SifraLogovanogRadnika();
            //        if (searchBy != null)
            //        {
            //            mycontext.persons
            //                .Where(t =>
            //                t.Firstname.Contains(search) ||
            //                t.Lastname.Contains(search) ||
            //                t.Description.Contains(search))
            //.ToList();
            //        }

            //Lista svih trebovanja ulogovanog korisnika koje je njegov sektor obavio
            var data = ListaTrebovanjaZaDT(sifraRadnika);

            //koristimo za prikaz svih elemenata ako je val = -1 vrati sve elemente
            if (take == -1)
            {
                take = data.Count();
            }

            var result = data
                           .OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
                           .Skip(skip)
                           .Take(take)
                           .ToList();

            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering

            filteredResultsCount = data.Count();
            totalResultsCount = data.Count();

            return result;
        }
      

       

        #endregion
    }
}