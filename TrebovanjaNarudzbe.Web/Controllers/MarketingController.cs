using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;
using X.PagedList;

namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class MarketingController : Controller
    {
        public string adresaHosta = @"http://localhost:51059/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;

        //Konstruktor za bazu
        public MarketingController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }

        public List<TrebovanjeViewModel> PopunjavanjeAtributaTrebovanja(List<Trebovanje> DbTrebovanja)
        {
            List<TrebovanjeViewModel> VmTrebovanja = DbTrebovanja.Select(t => new TrebovanjeViewModel
             {
                 TrebovanjeId = t.TrebovanjeId,
                 NapomenaRadnika = t.NapomenaRadnika,
                 DatumPodnesenogZahtjeva = t.DatumPodnosenjaZahtjeva,
                 SerijskiBroj = t.SerijskiBroj,
                 ImeIPrezimeRadnika = t.vRadniks.Ime + " " + t.vRadniks.Prezime,
                 ListaArtikalaTrebovanja = t.TrebovanjeVeznas.Where(tr => tr.TrebovanjeId == t.TrebovanjeId).Select(y => new ArtiklViewModel
                 {
                     ArtiklId = y.ArtikalId,
                     TrebovanaKolicina = y.TrebovanaKolicina,
                     RezervisanaKolicina = y.RezervisaniArtikli.RezervisanaKolicina,
                     Naziv = y.vInformacijeOArtiklu.Naziv,
                     Cijena = y.vInformacijeOArtiklu.Cijena,
                     NedostajucaKolicna = y.KolicinaKojaNedostaje,
                     Spremno = y.StatusArtiklaId == (int)Enum.Status.Preuzeto || y.StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje ? true : false,
                     Preuzeto = y.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false
                 }).ToList(),
                 NapomenaNadredjenog = t.NapomenaNadredjenog,
                 StatusTrebovanjaId = t.StatusTrebovanjaId,
                 NazivStatusa = t.Status.StatusNaziv,
                 DatumZaduzenjaTrebovanja = t.DatumZaduzenjaTrebovanja
             }).OrderByDescending(t => t.DatumPodnesenogZahtjeva).ToList();

            return VmTrebovanja;
        }

        #region Trebovanja marketing
        public List<TrebovanjeViewModel> VratiSvaTrebovanja() {
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(o => o.StatusTrebovanjaId != (int)Enum.Status.Na_čekanju 
            && o.StatusTrebovanjaId != (int)Enum.Status.U_procesu_odobravanja && o.StatusTrebovanjaId != (int)Enum.Status.Odbijeno).ToList();

            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);

            return VmTrebovanja;
        }

        [HandleError(View = "Error")]
        public ActionResult PrikazSvihTrebovanja(int? page)
        {
            var listaTrebovanja = VratiSvaTrebovanja();
            ViewBagStatusiTrebovanja();
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = listaTrebovanja.ToPagedList(pageNumber, 6);

            return View(ListaSaPaginacijom);
        }

        public void ViewBagStatusiTrebovanja()
        {
            ViewBag.ListaStatusa = trebovanjeNabavkeContext.Status.Select(x => new SelectListItem
            {
                Text = x.StatusNaziv
            }).ToList();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Metoda za vracanje listu aktivnih trebovanja za koju marketing izvrsava narudzbu
        [Authorize(Roles = "Referent skladista")]
        [HandleError(View = "Error")]
        public ActionResult PrikazSvihAktivnihTrebovanja(int? page)
        {
            var listaTrebovanja = ListaSvihAktivnihTrebovanja();
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = listaTrebovanja.ToPagedList(pageNumber, 6);
            return View(ListaSaPaginacijom);
        }

        public List<TrebovanjeViewModel> ListaSvihAktivnihTrebovanja()
        {
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(o => o.StatusTrebovanjaId == (int)Enum.Status.U_procesu_nabavke).ToList();
            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);

            return VmTrebovanja;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Provjera kolicine artikala iz trebovanja koje je vec odobreno
        public List<ArtiklViewModel> ProvjeraKolicineStanja(int trebovanjeId)
        {
            List<ArtiklViewModel> trebovaniArtikli = trebovanjeNabavkeContext.TrebovanjeVeznas.Where(t => t.TrebovanjeId == trebovanjeId).Select(y => new ArtiklViewModel
            {
                ArtiklId = y.ArtikalId,
                TrebovanaKolicina = y.TrebovanaKolicina,
                RezervisanaKolicina = y.RezervisaniArtikli.RezervisanaKolicina,
                Naziv = y.vInformacijeOArtiklu.Naziv,
                Cijena = y.vInformacijeOArtiklu.Cijena,
                NedostajucaKolicna = y.KolicinaKojaNedostaje,
                Spremno = y.StatusArtiklaId == (int)Enum.Status.Preuzeto || y.StatusArtiklaId == (int)Enum.Status.Spremno_za_preuzimanje ? true : false,
                Preuzeto = y.StatusArtiklaId == (int)Enum.Status.Preuzeto ? true : false
            }).ToList();

            return trebovaniArtikli;
        }

        //Metoda koja se poziva na dugme Naruceno. Mijenja status trebovanja i salje mejl dalje do skladista
        //POST Narucivanje
        [Authorize(Roles = "Referent skladista")]
        [HandleError(View = "Error")]
        public ActionResult Narucivanje(int trebovanje)
        {
            try
            {
                Trebovanje DbTrebovanje = trebovanjeNabavkeContext.Trebovanjes.First(x => x.TrebovanjeId == trebovanje);
                // &&(x.StatusTrebovanjaId==3 || x.vRadniks.NazivSektora=="M"));// slucaj ako neko ukuca tacan id trebovanja u URL a trebovanje je vec naruceno ili nije ni trebalo biti naruceno
                DbTrebovanje.StatusTrebovanjaId = (int)Enum.Status.Naruceno; //status trebovanja "naruceno"
                EmailController email = new EmailController();
                email.PosaljiMejlSkladistaruZaTrebvanje(DbTrebovanje.SifraRadnika, trebovanje);
                foreach (var artikl in DbTrebovanje.TrebovanjeVeznas)
                {
                    artikl.RezervisaniArtikli.TrenutnaKolicina += artikl.KolicinaKojaNedostaje;
                    artikl.KolicinaKojaNedostaje = 0;
                    
                }
                trebovanjeNabavkeContext.SaveChanges();
                return RedirectToAction("PrikazSvihAktivnihTrebovanja");
            }
            catch
            {
                return View("Error");
            }
        }
        //GET Narucivanje
        [HandleError(View = "Error")]
        [Authorize(Roles = "Referent skladista")]
        public ActionResult NovoTrebovanjeMarketing(int trebovanje) // preimenovati, salje se u linku mejla za jedno trebovanje koje treba naruciti
        {
            try
            {
                var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(o => o.TrebovanjeId == trebovanje).ToList();
                TrebovanjeViewModel VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja).First();
                return View(VmTrebovanja);
            }
            catch
            {
                return View("Error");
            }
        }

        #region DataTablesAktivanaTrebovanjaMarketing
        public ActionResult TabelarniPrikazSvihAktivnihTrebovanja()
        {
            return View();
        }

        //json lista za datatables
        public ActionResult ListaAktivnihTrebovanjaZaDT()
        {
            List<TrebovanjeViewModel> listaNabavki = ListaSvihAktivnihTrebovanja();

            var result = new List<TrebovanjeViewModel>();

            //filtriramo listu 
            foreach (var s in listaNabavki)
            {
                // simple remapping adding extra info to found dataset
                result.Add(new TrebovanjeViewModel
                {
                    TrebovanjeId = s.TrebovanjeId,
                    SerijskiBroj = s.SerijskiBroj,
                    SifraRadnika = s.SifraRadnika,
                    ImeIPrezimeRadnika = s.ImeIPrezimeRadnika,
                    DatumPodnesenogZahtjeva = s.DatumPodnesenogZahtjeva,
                    ListaArtikalaTrebovanja = s.ListaArtikalaTrebovanja


                });
            };
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }


        #endregion
        #endregion

        #region Nabavke

        //Metoda za vracanje listu aktivnih trebovanja za koju marketing izvrsava narudzbu
        [Authorize(Roles = "Referent nabavke")]
        public ActionResult PrikazSvihAktivnihNabavki(int? page)
        {
            List<NabavkaViewModel> listaNabavki = ListaAktivnihNabavki();
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = listaNabavki.ToPagedList(pageNumber, 6);
            return View(ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> ListaAktivnihNabavki()
        {
            var DBlistaNabavki = trebovanjeNabavkeContext.Nabavkes.Where(o => o.StatusNabavkeId == (int)Enum.Status.U_procesu_nabavke).ToList();
            List<NabavkaViewModel> listaNabavki = new List<NabavkaViewModel>();
            foreach (var nabavka in DBlistaNabavki)
            {
                var docs = nabavka.Dokuments;
                List<DokumentViewModel> dokumenti = new List<DokumentViewModel>();
                foreach (var dokument in docs)
                {
                    dokumenti.Add(new DokumentViewModel(dokument.Dokument1, dokument.Naziv, dokument.DokumentId));
                }
                listaNabavki.Add(new NabavkaViewModel()
                {
                    NabavkaId = nabavka.NabavkaId,
                    SerijskiBroj = nabavka.SerijskiBroj,
                    NapomenaReferentaNabavke = nabavka.NapomenaReferentaNabavke,
                    NapomenaSefa = nabavka.NapomenaSefa,
                    DatumPodnosenjaZahtjeva = nabavka.DatumPodnosenjaZahtjeva,
                    DatumPreuzimanja = nabavka.DatumPreuzimanja,
                    SifraRadnika = nabavka.SifraRadnika,
                    Obrazlozenja = nabavka.Obrazlozenja,
                    Odgovoran = nabavka.Odgovoran,
                    SifraReferentaNabavke = nabavka.SifraReferentaNabavke,
                    StatusNabavkeId = nabavka.StatusNabavkeId,
                    NazivStatusa = nabavka.Status.StatusNaziv,
                    TipId = nabavka.TipId,
                    VezanaNabavkaId = nabavka.VezanaNabavkaId,
                    ImeIPrezimeRadnika = nabavka.vRadnik.Ime + " " + nabavka.vRadnik.Prezime,
                    //Kupljenje dokumenata iz baze za traženu nabavku
                    Dokumenti = dokumenti,
                    //Kupljenje stavki iz baze za traženu nabavku
                    Stavke = nabavka.NabavkaVeznas.Select(s => new NabavkaVeznaViewModel
                    {
                        NabavkaVeznaId = s.NabavkaVeznaId,
                        Kolicina = s.Kolicina,
                        Cijena = s.Cijena,
                        Dobavljac = s.Dobavljac,
                        Opis = s.Opis
                    }).ToList()
                });
            }
            return listaNabavki;
        }

        //Metoda koja se poziva na dugme Naruceno. Mijenja status nabavke i salje mejl dalje do skladista
        [Authorize(Roles = "Referent nabavke")]
        [HandleError(View = "Error")]
        public bool NabavkaNarucivanje(int nabavka, string napomena)
        {
            Nabavke nabavke = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(x => x.NabavkaId == nabavka);
            nabavke.StatusNabavkeId = (int)Enum.Status.Naruceno; //status trebovanja "naruceno"
            nabavke.NapomenaReferentaNabavke = napomena;
            OdobravanjeNabavkeController email = new OdobravanjeNabavkeController();
            if (email.PosaljiMejlSkladistaruZaNovuNabavku(nabavke.SifraRadnika, nabavka))
            {
                trebovanjeNabavkeContext.SaveChanges();
                return true;
            }
            else
                return false;
        }

        #endregion     

        #region FilteriZaMarketing
        ///////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        //[HttpPost]
        public ActionResult FilterTrebovanje(string SortiranjePoImenu, string SortiranjeDatum, int? page, string StatusTrebovanja)
        {

            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;
            ViewBag.StatusTrebovanja = StatusTrebovanja;

            //f-ja koja pravi viewBag statusi trebovanja za DropDownList na View-u
            ViewBagStatusiTrebovanja();

            List<TrebovanjeViewModel> listaTrebovanja = VratiSvaTrebovanja();

            Helper.FilterTrebovanja filter = new Helper.FilterTrebovanja(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, listaTrebovanja);


            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);
            return View("PrikazSvihTrebovanja", ListaSaPaginacijom);


        }
        public ActionResult FilterAktivnaTrebovanja(string SortiranjePoImenu, string SortiranjeDatum, int? page)
        {

            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;

            List<TrebovanjeViewModel> listaTrebovanja = ListaSvihAktivnihTrebovanja();


            Helper.FilterTrebovanja filter = new Helper.FilterTrebovanja(SortiranjePoImenu, SortiranjeDatum, listaTrebovanja);


            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);
            return View("PrikazSvihAktivnihTrebovanja", ListaSaPaginacijom);


        }




        #endregion
        #region FilteriZaMarketingNabavke
        public ActionResult FilterAktivneNabavke(string SortiranjePoImenu, string SortiranjeDatum, int? page)
        {

            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;
           
            List<NabavkaViewModel> listaAktivnihNabavki = ListaAktivnihNabavki();

            Helper.FilterNabavke filter = new Helper.FilterNabavke(SortiranjePoImenu, SortiranjeDatum, listaAktivnihNabavki);


            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);
            return View("PrikazSvihAktivnihNabavki", ListaSaPaginacijom);


        }

    
        #endregion

        #region DataTableZavrsenaTrebovanja
        public ActionResult TabelarniPrikazSvihTrebovanja()
        {
            return View();

        }
        //json lista
        public ActionResult ListaSvihTrebovanjaZaMarketingDT()
        {
            List<TrebovanjeViewModel> listaTrebovanja = VratiSvaTrebovanja();

            var result = new List<TrebovanjeViewModel>();

            //filtriramo listu 
            foreach (var t in listaTrebovanja)
            {
                // simple remapping adding extra info to found dataset
                result.Add(new TrebovanjeViewModel
                {
                    SerijskiBroj = t.SerijskiBroj,
                    SifraRadnika = t.SifraRadnika,
                    ImeIPrezimeRadnika = t.ImeIPrezimeRadnika,
                    DatumPodnesenogZahtjeva = t.DatumPodnesenogZahtjeva,
                    ListaArtikalaTrebovanja = t.ListaArtikalaTrebovanja
                });
            };

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        
        #endregion
    }
}