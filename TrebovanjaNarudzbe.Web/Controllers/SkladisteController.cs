using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;
using X.PagedList;
using System.Text;
using System.Globalization;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class SkladisteController : Controller
    {
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;
        public SkladisteController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }

        private List<TrebovanjeViewModel> PopunjavanjeAtributaTrebovanja(List<Trebovanje> DbTrebovanja)
        {
            List<TrebovanjeViewModel> VmTrebovanja =  DbTrebovanja.Select(t => new TrebovanjeViewModel()
             {
                 DatumPodnesenogZahtjeva = t.DatumPodnosenjaZahtjeva,
                 DatumZaduzenjaTrebovanja=t.DatumZaduzenjaTrebovanja,
                 ImeIPrezimeRadnika = t.vRadniks.Ime + " " + t.vRadniks.Prezime,
                 TrebovanjeId = t.TrebovanjeId,
                 NapomenaRadnika = t.NapomenaRadnika,
                 NapomenaNadredjenog = t.NapomenaNadredjenog,
                 SifraRadnika = t.SifraRadnika,
                 SerijskiBroj = t.SerijskiBroj,
                 ListaArtikalaTrebovanja = t.TrebovanjeVeznas.Select(tv => new ArtiklViewModel()
                 {
                     ArtiklId = tv.ArtikalId,
                     TrebovanaKolicina = tv.TrebovanaKolicina,
                     Naziv = tv.vInformacijeOArtiklu.Naziv,
                     Spremno = tv.StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje || tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                     Preuzeto = tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                 }).OrderBy(o => o.Naziv).ToList()

             }).OrderByDescending(o => o.DatumPodnesenogZahtjeva).ToList();

            return VmTrebovanja;
        }

        #region Trebovanja
        public List<TrebovanjeViewModel> GetAktivnaTrebovanja()
        {
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.StatusTrebovanjaId == (int)Enum.Status.Odobreno ||
            t.StatusTrebovanjaId == (int)Enum.Status.Spremno_za_preuzimanje || t.StatusTrebovanjaId == (int)Enum.Status.Naruceno).ToList();

            List<TrebovanjeViewModel> VmTrebovanja =  PopunjavanjeAtributaTrebovanja(DbTrebovanja);

            return VmTrebovanja;
        }

        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult ZavrsenaTrebovanja(int? page)
        {
            try
            {
                List<TrebovanjeViewModel> listaZavrsenihTrebovanja = ListaZavrsenihTrebovanja();
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var ListaSaPaginacijom = listaZavrsenihTrebovanja.ToPagedList(pageNumber, 6); // will only contain 25 products max because of the pageSize

                return View(ListaSaPaginacijom);
            }
            catch 
            {
                return View("Error");
            }
        }

        public List<TrebovanjeViewModel> ListaZavrsenihTrebovanja()
        {
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.StatusTrebovanjaId == (int)Enum.Status.Preuzeto).ToList();

            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);

            return VmTrebovanja;
        }

        //[HttpPost]
        #region filterZaTrebovanja Aktivna i Zavrsena

        public ActionResult FilterTrebovanje(string SortiranjePoImenu, string SortiranjeDatum, int? page)
        {
           

            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;

            List <TrebovanjeViewModel> listaTrebovanja = GetAktivnaTrebovanja();

            //filter za trebovanja 
            Helper.FilterTrebovanja filter = new Helper.FilterTrebovanja(SortiranjePoImenu, SortiranjeDatum, listaTrebovanja);

             
            //listaTrebovanja = Helper.FilterTrebovanja.FilterZaTrebovanja(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, listaTrebovanja);

            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);
            return View("AktivnaTrebovanja", ListaSaPaginacijom);


        }


        public ActionResult FilterZavrsenaTrebovanja(string SortiranjePoImenu, string SortiranjeDatum, int? page)
        {


            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;           

            List<TrebovanjeViewModel> listaTrebovanja = ListaZavrsenihTrebovanja();  
            
            //filter pravimo instancu klase i saljemo paramerte po cemu sve zelimo filtrirati
            Helper.FilterTrebovanja filter = new Helper.FilterTrebovanja(SortiranjePoImenu, SortiranjeDatum, listaTrebovanja);

            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);
            return View("ZavrsenaTrebovanja", ListaSaPaginacijom);


        }
     
        #endregion


        

        // GET: Skladiste
        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult AktivnaTrebovanja(int? page)
        {
            List<TrebovanjeViewModel> aktivnaTrebovanja = GetAktivnaTrebovanja();
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var ListaSaPaginacijom = aktivnaTrebovanja.ToPagedList(pageNumber, 6); // will only contain 25 products max because of the pageSize

            return View(ListaSaPaginacijom);
        }


        //Provjera da li su svi artikli isporuceni
        public bool TrebovanjePreuzeto(TrebovanjeViewModel viewModel)
        {
            return viewModel.ListaArtikalaTrebovanja.All(a => a.Preuzeto);
        }


        [HttpPost]
        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public JsonResult IzmjenaAktivnogTrebovanja(TrebovanjeViewModel viewModel)
        {
            viewModel.ListaArtikalaTrebovanja = viewModel.ListaArtikalaTrebovanja.OrderBy(a => a.ArtiklId).ToList();
            List<string> izmjenjeniArtikli = new List<string>();
            var trebovanjeVeze = trebovanjeNabavkeContext.TrebovanjeVeznas.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).OrderBy(a => a.ArtikalId).ToList();
            List<TrebovanjeVezna> artikliZaskidanjeSaStanja = new List<TrebovanjeVezna>();

            for (int i = 0; i < viewModel.ListaArtikalaTrebovanja.Count; i++)
            {
                bool spremno = false;
                bool preuzeto = false;

                //stanje iz baze konvertujem u bool
                if (trebovanjeVeze[i].StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje)
                {
                    spremno = true;
                    preuzeto = false;
                }
                else if (trebovanjeVeze[i].StatusArtiklaId == (int)Enum.Status.Preuzeto)
                {
                    spremno = true;
                    preuzeto = true;
                }
                if (viewModel.ListaArtikalaTrebovanja[i].Spremno != spremno && viewModel.ListaArtikalaTrebovanja[i].Spremno)
                {
                    int artikalId = viewModel.ListaArtikalaTrebovanja[i].ArtiklId;
                    trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].StatusArtiklaId = (int)Enum.Status.Spremno_za_preuzimanje;
                    izmjenjeniArtikli.Add(trebovanjeNabavkeContext.vInformacijeOArtiklus.FirstOrDefault(a => a.ArtikalId == artikalId).Naziv);
                }
                else if (viewModel.ListaArtikalaTrebovanja[i].Spremno != spremno && !viewModel.ListaArtikalaTrebovanja[i].Spremno)
                {
                    trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi;
                }


                if (viewModel.ListaArtikalaTrebovanja[i].Preuzeto != preuzeto && viewModel.ListaArtikalaTrebovanja[i].Preuzeto)
                {
                    trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].StatusArtiklaId = (int)Enum.Status.Preuzeto;
                    artikliZaskidanjeSaStanja.Add(new TrebovanjeVezna
                    {
                        ArtikalId = trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].ArtikalId,
                        TrebovanaKolicina = trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].TrebovanaKolicina
                    });
                }
                else if (viewModel.ListaArtikalaTrebovanja[i].Preuzeto != preuzeto && !viewModel.ListaArtikalaTrebovanja[i].Preuzeto && viewModel.ListaArtikalaTrebovanja[i].Spremno)
                {
                    trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].StatusArtiklaId = (int)Enum.Status.Spremno_za_preuzimanje;
                }
                else if (viewModel.ListaArtikalaTrebovanja[i].Preuzeto != preuzeto && !viewModel.ListaArtikalaTrebovanja[i].Preuzeto && !viewModel.ListaArtikalaTrebovanja[i].Spremno)
                {
                    trebovanjeVeze.Where(t => t.TrebovanjeId == viewModel.TrebovanjeId).ToList()[i].StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi;
                }

            }
            foreach (var artikal in artikliZaskidanjeSaStanja)
            {
                trebovanjeNabavkeContext.RezervisaniArtiklis.FirstOrDefault(a => a.ArtikalId == artikal.ArtikalId).RezervisanaKolicina -= artikal.TrebovanaKolicina;
                trebovanjeNabavkeContext.RezervisaniArtiklis.FirstOrDefault(a => a.ArtikalId == artikal.ArtikalId).TrenutnaKolicina -= artikal.TrebovanaKolicina;
            }
            trebovanjeNabavkeContext.SaveChanges();
            if (izmjenjeniArtikli.Count > 0)
            {
                EmailController email = new EmailController();
                string naslovEmaila = "Artikli spremni za preuzimanje:";
                string naMejl = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t =>
                                t.TrebovanjeId == viewModel.TrebovanjeId).vRadniks.Email;
                string sadrzajEmaila = "<p>Poštovani, <br/><br/>" +
                    " <p>Sljedeći artikli koje ste <a href=" + email.adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" + viewModel.TrebovanjeId + ">trebovali </a>" +
                    " su spremni za preuzimanje: </p> <ul>";
                foreach (var artikal in izmjenjeniArtikli)
                {
                    sadrzajEmaila += "<li>" + artikal + "</li>";
                }
                sadrzajEmaila += "</ul><br>  <span> Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke </span> ";
                email.PosaljiMejl(naslovEmaila, naMejl, sadrzajEmaila);
                //toster za uspjesno poslan mejl kad je artikl spreman

            }
            if (TrebovanjePreuzeto(viewModel))
            {
                var trebovanje = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t => viewModel.TrebovanjeId == t.TrebovanjeId);
                trebovanje.StatusTrebovanjaId = (int)Enum.Status.Preuzeto;
                trebovanje.DatumZaduzenjaTrebovanja = DateTime.Now;
                trebovanjeNabavkeContext.SaveChanges();
                EmailController email = new EmailController();
                string naslovEmaila = "Potvrda o preuzimanju trebovanja";
                string naMejl = trebovanje.vRadniks.Email;
                string sadrzajEmaila = "<p>Poštovani, <br/><br/> Vaše <a href=" + email.adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                viewModel.TrebovanjeId + ">trebovanje </a> je preuzeto. </p><br/><br/> ";
                sadrzajEmaila += "<span>Srdačan pozdrav, <br/><br/>Lanaco trebovanje i nabavke</span>";

                email.PosaljiMejl(naslovEmaila, naMejl, sadrzajEmaila);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

            //ako je izmjenjeniArtikli.Count > 0 posalji mail 
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult PrikazJednogTrebovanja(int trebovanje)
        {
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.TrebovanjeId == trebovanje).ToList();

            TrebovanjeViewModel VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja).FirstOrDefault();

            return View(VmTrebovanja);
        }

        #endregion

        #region DataTables
        public ActionResult TabelarniPrikazZavrsenihTrebovanja()
        {
            return View();

        }
        //json lista zavrsenih trebovanja za datatable
        public ActionResult ListaZavrsenihTrebovanjaZaDT()
        {
            List<TrebovanjeViewModel> listaZavrsenihTrebovanja = ListaZavrsenihTrebovanja();                      
            return Json(new { data = listaZavrsenihTrebovanja }, JsonRequestBehavior.AllowGet);
        }


        ////// tabelarni prikaz AKTIVNIH TREBOVANJA u skladistu //////
        public ActionResult TabelarniPrikazAktivnihTrebovanja()
        {
            return View();

        }
        //json lista aktivnih trebovanja za datatable
        public ActionResult ListaAktivnihTrebovanjaZaDT()
        {
            List<TrebovanjeViewModel> listaZavrsenihTrebovanja = GetAktivnaTrebovanja();
            return Json(new { data = listaZavrsenihTrebovanja }, JsonRequestBehavior.AllowGet);
        }

        //public PartialViewResult AktivnoTrebovanjePartial(string jsonTrebovanje) // za modal na click na red u datatable-u
        //{
        //    TrebovanjeViewModel viewModel = JsonConvert.DeserializeObject<TrebovanjeViewModel>(jsonTrebovanje);
        //    return PartialView("_AktivnoTrebovanjeSkladiste", viewModel);
        //}

        public PartialViewResult AktivnoTrebovanjePartial(string serijskiBroj)
        {
            var tr = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t => t.SerijskiBroj == serijskiBroj);

            TrebovanjeViewModel trebovanjeViewModel =  new TrebovanjeViewModel {
                DatumPodnesenogZahtjeva = tr.DatumPodnosenjaZahtjeva,
                DatumZaduzenjaTrebovanja = tr.DatumZaduzenjaTrebovanja,
                ImeIPrezimeRadnika = tr.vRadniks.Ime + " " + tr.vRadniks.Prezime,
                TrebovanjeId = tr.TrebovanjeId,
                NapomenaRadnika = tr.NapomenaRadnika,
                NapomenaNadredjenog = tr.NapomenaNadredjenog,
                SifraRadnika = tr.SifraRadnika,
                SerijskiBroj = tr.SerijskiBroj,
                ListaArtikalaTrebovanja = tr.TrebovanjeVeznas.Select(tv => new ArtiklViewModel()
                {
                    ArtiklId = tv.ArtikalId,
                    TrebovanaKolicina = tv.TrebovanaKolicina,
                    Naziv = tv.vInformacijeOArtiklu.Naziv,
                    Spremno = tv.StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje || tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                    Preuzeto = tv.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false,
                }).OrderBy(o => o.Naziv).ToList()
            };
            return PartialView("_AktivnoTrebovanjeSkladiste", trebovanjeViewModel);
        }
        ////// tabelarni prikaz AKTIVNIH TREBOVANJA u skladistu //////



        ////// tabelarni prikaz svih AKTIVNIH NABAVKI u skladistu //////
        ////// tabelarni prikaz svih AKTIVNIH NABAVKI u skladistu //////
        public ActionResult TabelarniPrikazAktivneNabavke()
        {
            return View();

        }

        public List<NabavkaViewModel> AktivneNabavkeZaSkladiste() {

            return trebovanjeNabavkeContext.Nabavkes.Where(t => t.StatusNabavkeId == (int)Enum.Status.Odobreno
            || t.StatusNabavkeId == (int)Enum.Status.Spremno_za_preuzimanje || t.StatusNabavkeId == (int)Enum.Status.Naruceno).Select(n => new NabavkaViewModel
            {
                SerijskiBroj = n.SerijskiBroj,
                SifraRadnika = n.SifraRadnika,
                DatumPodnosenjaZahtjeva = n.DatumPodnosenjaZahtjeva,
                ImeIPrezimeRadnika = n.vRadnik.Ime + " " + n.vRadnik.Prezime
                
            }).ToList();

        }
        public JsonResult ListaAktivnihNabavkiZaDT()//json lista aktivnih nabavki za datatable, koristi se kao source u skripti za datatable
        {
            List<NabavkaViewModel> listaSvihAktivnihNabavki = AktivneNabavkeZaSkladiste();
            return Json(new { data = listaSvihAktivnihNabavki }, JsonRequestBehavior.AllowGet);
        }

        //public PartialViewResult AktivnaNabavkaPartial(string jsonNabavka) // za modal na click na red u datatable-u
        //{
        //    NabavkaViewModel viewModel = JsonConvert.DeserializeObject<NabavkaViewModel>(jsonNabavka);

        //    return PartialView("_AktivnaNabavkaSkladiste", viewModel);
        //}

        public PartialViewResult AktivnaNabavkaPartial(string serijskiBroj)
        {
            var n = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.SerijskiBroj == serijskiBroj);

            NabavkaViewModel nabavkaViewModel = new NabavkaViewModel
            {
                NabavkaId = n.NabavkaId,
                SerijskiBroj = n.SerijskiBroj,
                NapomenaReferentaNabavke = n.NapomenaReferentaNabavke == null ? "" : n.NapomenaReferentaNabavke,
                NapomenaSefa = n.NapomenaSefa == null ? "" : n.NapomenaSefa,
                DatumPodnosenjaZahtjeva = n.DatumPodnosenjaZahtjeva,
                DatumPreuzimanja = n.DatumPreuzimanja,
                SifraRadnika = n.SifraRadnika,
                Obrazlozenja = n.Obrazlozenja == null ? "" : n.Obrazlozenja,
                Odgovoran = n.Odgovoran,
                SifraReferentaNabavke = n.SifraReferentaNabavke,
                StatusNabavkeId = n.StatusNabavkeId,
                NazivStatusa = n.Status.StatusNaziv,
                TipId = n.TipId,
                VezanaNabavkaId = n.VezanaNabavkaId,
                ImeIPrezimeRadnika = n.vRadnik.Ime + " " + n.vRadnik.Prezime,
                TipNabavke = n.TipNabavke.Naziv,

                //Kupljenje dokumenata iz baze za traženu nabavku
                Dokumenti = n.Dokuments.Select(d => new DokumentViewModel(d.Dokument1, d.Naziv, d.DokumentId)).ToList(),
                //Kupljenje stavki iz baze za traženu nabavku
                Stavke = n.NabavkaVeznas.Select(s => new NabavkaVeznaViewModel
                {
                    NabavkaVeznaId = s.NabavkaVeznaId,
                    Kolicina = s.Kolicina,
                    Cijena = s.Cijena,
                    Dobavljac = s.Dobavljac,
                    Spremno = s.StatusId == (int)Enum.Status.Spremno_za_preuzimanje || s.StatusId == (int)Enum.Status.Preuzeto ? true : false,
                    Preuzeto = s.StatusId == (int)Enum.Status.Preuzeto ? true : false,
                    Opis = s.Opis
                }).ToList()
            };
            return PartialView("_AktivnaNabavkaSkladiste", nabavkaViewModel);
        }
        ////// tabelarni prikaz svih AKTIVNIH NABAVKI u skladistu //////
        ////// tabelarni prikaz svih AKTIVNIH NABAVKI u skladistu //////
        #endregion

        #region Nabavke

        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult PrikazNabavke(int nabavka)
        {
            List<Nabavke> DbNabavke = trebovanjeNabavkeContext.Nabavkes.Where(t => t.NabavkaId == nabavka).ToList();
            OdobravanjeNabavkeController odobravanjeNabavke = new OdobravanjeNabavkeController();
            NabavkaViewModel VmNabavke = odobravanjeNabavke.PopunjavanjeAtributa(DbNabavke).FirstOrDefault();

            return View(VmNabavke);
        }

        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult AktivneNabavke(int? page)
        {
            List<NabavkaViewModel> aktivneNabavke = VratiAktivneNabavke();
            var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            var ListaSaPaginacijom = aktivneNabavke.ToPagedList(pageNumber, 6); // will only contain 25 products max because of the pageSize

            return View(ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> VratiAktivneNabavke()
        {
            List<Nabavke> DbNabavke = trebovanjeNabavkeContext.Nabavkes.Where(t => t.StatusNabavkeId == (int)Enum.Status.Odobreno 
            || t.StatusNabavkeId == (int)Enum.Status.Spremno_za_preuzimanje || t.StatusNabavkeId == (int)Enum.Status.Naruceno).ToList();

            OdobravanjeNabavkeController odobravanjeNabavke = new OdobravanjeNabavkeController();
            List<NabavkaViewModel> VmNabavke = odobravanjeNabavke.PopunjavanjeAtributa(DbNabavke);

            return VmNabavke;
        }


        [HttpPost]
        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public JsonResult IzmjenaAktivneNabavke(NabavkaViewModel viewModel)
        {
            viewModel.Stavke = viewModel.Stavke.OrderBy(a => a.NabavkaVeznaId).ToList();
            List<string> izmjenjeniArtikli = new List<string>();
            var nabavkaVezna = trebovanjeNabavkeContext.NabavkaVeznas.Where(t => t.NabavkaId == viewModel.NabavkaId).OrderBy(a => a.NabavkaVeznaId).ToList();

            for (int i = 0; i < viewModel.Stavke.Count; i++)
            {
                bool spremno = false;
                bool preuzeto = false;

                //stanje iz baze konvertujem u bool
                if (nabavkaVezna[i].StatusId == (int)Enum.Status.Spremno_za_preuzimanje)
                {
                    spremno = true;
                    preuzeto = false;
                }
                else if (nabavkaVezna[i].StatusId == (int)Enum.Status.Preuzeto)
                {
                    spremno = true;
                    preuzeto = true;
                }
                if (viewModel.Stavke[i].Spremno != spremno && viewModel.Stavke[i].Spremno)
                {
                    int artikalId = viewModel.Stavke[i].NabavkaVeznaId;
                    nabavkaVezna.Where(t => t.NabavkaId == viewModel.NabavkaId).ToList()[i].StatusId = (int)Enum.Status.Spremno_za_preuzimanje;
                    izmjenjeniArtikli.Add(trebovanjeNabavkeContext.NabavkaVeznas.FirstOrDefault(t => t.NabavkaVeznaId == artikalId).Opis);
                }
                else if (viewModel.Stavke[i].Spremno != spremno && !viewModel.Stavke[i].Spremno)
                {
                    nabavkaVezna.Where(t => t.NabavkaId == viewModel.NabavkaId).ToList()[i].StatusId = (int)Enum.Status.Artikl_u_pripremi;
                }


                if (viewModel.Stavke[i].Preuzeto != preuzeto && viewModel.Stavke[i].Preuzeto)
                {
                    nabavkaVezna.Where(t => t.NabavkaId == viewModel.NabavkaId).ToList()[i].StatusId = (int)Enum.Status.Preuzeto;
                   
                }
                else if (viewModel.Stavke[i].Preuzeto != preuzeto && !viewModel.Stavke[i].Preuzeto && viewModel.Stavke[i].Spremno)
                {
                    nabavkaVezna.Where(t => t.NabavkaId == viewModel.NabavkaId).ToList()[i].StatusId = (int)Enum.Status.Spremno_za_preuzimanje;
                }
                else if (viewModel.Stavke[i].Preuzeto != preuzeto && !viewModel.Stavke[i].Preuzeto && !viewModel.Stavke[i].Spremno)
                {
                    nabavkaVezna.Where(t => t.NabavkaId == viewModel.NabavkaId).ToList()[i].StatusId = (int)Enum.Status.Artikl_u_pripremi;
                }

            }

            trebovanjeNabavkeContext.SaveChanges();
            if (izmjenjeniArtikli.Count > 0)
            {
                EmailController email = new EmailController();
                string naslovEmaila = "Artikli iz nabavke su spremni za preuzimanje";
                string naMejl = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.NabavkaId == viewModel.NabavkaId).vRadnik.Email;
                string sadrzajEmaila = "<p>Poštovani, <br/><br/>" +
                    " <p>Sljedeći artikli koje ste <a href=" + email.adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" + viewModel.NabavkaId + ">naručili </a>" +
                    " su spremni za preuzimanje: </p> <ul>";
                foreach (var artikal in izmjenjeniArtikli)
                {
                    sadrzajEmaila += "<li>" + artikal + "</li>";
                }
                sadrzajEmaila += "</ul><br/><span> Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke </span> ";
                email.PosaljiMejl(naslovEmaila, naMejl, sadrzajEmaila);
                //toster za uspjesno poslan mejl kad je artikl spreman

            }
            if (NabavkaPreuzeta(viewModel))
            {
                var nabavka = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => viewModel.NabavkaId == t.NabavkaId);
                nabavka.StatusNabavkeId = (int)Enum.Status.Preuzeto;
                //nabavka.DatumZaduzenjaTrebovanja = DateTime.Now; // trenutno ne postoji u bazi
                trebovanjeNabavkeContext.SaveChanges();
                EmailController email = new EmailController();
                string naslovEmaila = "Potvrda o preuzimanju trebovanja";
                string naMejl = nabavka.vRadnik.Email;
                string sadrzajEmaila = "<p>Poštovani, <br/><br/> Vaša <a href=" + email.adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" +
                                viewModel.NabavkaId + ">nabavka </a> je preuzeta. </p><br/><br/> ";
                sadrzajEmaila += "<span>Srdačan pozdrav, <br/><br/>Lanaco trebovanje i nabavke</span>";

                email.PosaljiMejl(naslovEmaila, naMejl, sadrzajEmaila);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

            //ako je izmjenjeniArtikli.Count > 0 posalji mail 
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        //Provjera da li su svi artikli isporuceni
        public bool NabavkaPreuzeta(NabavkaViewModel viewModel)
        {
            return viewModel.Stavke.All(a => a.Preuzeto);
        }

        //[HttpPost]
        public ActionResult FilterNabavke(string SortiranjePoImenu, string SortiranjeDatum, int? page)
        {
            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;

            List<NabavkaViewModel> listaNabavki = VratiAktivneNabavke();


            //sortiranje po nazivu artikla i imenu korisnika
            if (!string.IsNullOrEmpty(SortiranjePoImenu))
            {
                listaNabavki = FilterNabavkiArtikliKorisnici(SortiranjePoImenu, listaNabavki);
            }

            //najstarijeg ka najnovijem  == 1
            //najnovijeg ka najstarijem == 2
            //validacija-provjera vrijednosti inputa za datum
            if (SortiranjeDatum == "1" || SortiranjeDatum == "2")
            {
                listaNabavki = FilterNabavkiPoDatumu(listaNabavki, SortiranjeDatum);
            }
            
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = listaNabavki.ToPagedList(pageNumber, 6);
            return View("AktivneNabavke", ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> FilterNabavkiPoDatumu(List<NabavkaViewModel> listaNabavki, string SortiranjeDatum)
        {
            //filter od najstarijeg ka najnovijem  == 1
            //filter od najnovijeg ka najstarijem ==  2 
            switch (SortiranjeDatum)
            {
                case "1":
                    listaNabavki = listaNabavki.OrderBy(o => o.DatumPodnosenjaZahtjeva).ToList();
                    break;
                case "2":
                    listaNabavki = listaNabavki.OrderByDescending(o => o.DatumPodnosenjaZahtjeva).ToList();
                    break;
            }
            return listaNabavki;
        }

        public List<NabavkaViewModel> FilterNabavkiArtikliKorisnici(string txtPretraga, List<NabavkaViewModel> aktivneNabavke)
        {
            txtPretraga = txtPretraga.Trim();
            string[] rijeci = txtPretraga.Split(' ');

            //var aktivnaTrebovanja = GetAktivnaTrebovanja(); //Sva aktivna trebovanja, treba ih filtrirati..
            List<NabavkaViewModel> filtriranaNabavka = new List<NabavkaViewModel>();
            if (rijeci.Length == 2) //pretpostavka da je ukucano ime i prezime
            {
                filtriranaNabavka = aktivneNabavke.Where(t => Helper.FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(Helper.FilterTrebovanja.VratiNormalizovanString(rijeci[0])) &&
                                                                    Helper.FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(Helper.FilterTrebovanja.VratiNormalizovanString(rijeci[1]))).ToList();
            }
            if (filtriranaNabavka.Count == 0)
            {
                filtriranaNabavka = aktivneNabavke.Where(t => Helper.FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(Helper.FilterTrebovanja.VratiNormalizovanString(txtPretraga)) ||
                t.Stavke.Any(a => Helper.FilterTrebovanja.VratiNormalizovanString(a.Opis).Contains(Helper.FilterTrebovanja.VratiNormalizovanString(txtPretraga)))).ToList();///
            }
            return filtriranaNabavka;

        }

        [Authorize(Roles = "Skladistar")]
        [HandleError(View = "Error")]
        public ActionResult ZavrseneNabavke(int? page)
        {
            List<NabavkaViewModel> listaZavrsenihNabavki = ListaZavrsenihNabavki();
            var pageNumber = page ?? 1; 
            var ListaSaPaginacijom = listaZavrsenihNabavki.ToPagedList(pageNumber, 6); 

            return View(ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> ListaZavrsenihNabavki()
        {
            List<Nabavke> DbNabavke = trebovanjeNabavkeContext.Nabavkes.Where(t => t.StatusNabavkeId == (int)Enum.Status.Preuzeto).ToList();

            OdobravanjeNabavkeController odobravanjeNabavke = new OdobravanjeNabavkeController();

            List<NabavkaViewModel> VmNabavke = odobravanjeNabavke.PopunjavanjeAtributa(DbNabavke);

            return VmNabavke;
        }
         
        #endregion
    }
}