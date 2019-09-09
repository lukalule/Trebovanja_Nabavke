using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;
using TrebovanjaNarudzbe.Web.Enum;
using X.PagedList;
using Status = TrebovanjaNarudzbe.Web.Enum.Status;

namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class NabavkeController : Controller
    {
        readonly string adresaHosta = @"http://localhost:51059/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;

        public NabavkeController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }

        // GET: Nabavke - osnovno sredstvo
        public ActionResult NovaNabavkaZaOsnovnaSredstva()
        {
            ReferentiNabavke();
            Odgovorni();

            return View();
        }

        //Lista radnika za biranje osobe koja je referent za nabavku
        public void ReferentiNabavke()
        {
            ViewBag.Referenti = trebovanjeNabavkeContext.ReferentNabavkes.Select(f => new SelectListItem()
            {
                Value = f.SifraRadnika,
                Text = f.vRadnik.Ime + " " + f.vRadnik.Prezime + " - " + f.Napomena
            }).OrderBy(r => r.Text).ToList();
        }

        //Lista radnika za biranje osobe koja je odgovorna za nabavku
        public void Odgovorni()
        {
            //prikazuje samo one radnike kojima je ta osoba nadredjena, ovdje treba ukljuciti rolu radnika iz tima koji moze da pravi nabavku
            //var radnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(t => t.KorisnickoIme == User.Identity.Name).RadnikSifra;
            //ViewBag.Odgovorni = trebovanjeNabavkeContext.vRadniks.Where(t => t.Nivo1OdobravanjaSifra == radnik 
            //|| t.Nivo2OdobravanjaSifra == radnik || t.Nivo3OdobravanjaSifra == radnik || t.RadnikSifra == radnik)
            //.Select(o => new SelectListItem
            //{
            //    Value = o.RadnikSifra,
            //    Text = o.Ime + " " + o.Prezime
            //}).OrderBy(r => r.Text).ToList();

            ViewBag.Odgovorni = trebovanjeNabavkeContext.vRadniks.Select(o => new SelectListItem
            {
                Value = o.RadnikSifra,
                Text = o.Ime + " " + o.Prezime
            }).OrderBy(r => r.Text).ToList();
        }

        private bool ValidateFiles(List<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                if (file.FileName.EndsWith(".exe") || file.FileName.EndsWith(".app") || file.FileName.EndsWith(".bat"))
                    return false;
            }
            return true;
        }
        [HttpPost]
        [HandleError(View = "Error")]
        public ActionResult NovaNabavkaZaOsnovnaSredstva(NabavkaViewModel viewModel)
        {
            //if (!ModelState.IsValid)
            //    return View(viewModel);

            string serijskiBroj = NoviSerijskiBroj();

            List<DokumentViewModel> dokumenti = new List<DokumentViewModel>();

            if (viewModel.Files != null)
                foreach (HttpPostedFileBase file in viewModel.Files)
                {
                    //Checking file is available to save.
                    if (file != null)
                    {
                        byte[] uploadedFile = new byte[file.ContentLength];
                        file.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

                        dokumenti.Add(new DokumentViewModel(uploadedFile, file.FileName));
                    }
                }

            var logovaniRadnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(r => r.KorisnickoIme == User.Identity.Name);

            Nabavke novaNabavka = new Nabavke
            {
                SifraRadnika = logovaniRadnik.RadnikSifra,
                SifraReferentaNabavke = viewModel.SifraReferentaNabavke,
                TipId = viewModel.TipId,
                StatusNabavkeId = (int)Status.Na_čekanju,
                SerijskiBroj = serijskiBroj,
                DatumPodnosenjaZahtjeva = DateTime.Now,
                Obrazlozenja = viewModel.Obrazlozenja,
                Odgovoran = viewModel.Odgovoran,
                VezanaNabavkaId = viewModel.VezanaNabavkaId,
                Dokuments = dokumenti.Select(d => new Dokument
                {
                    Naziv = d.FileName,
                    Dokument1 = d.fileBytes
                }).ToList(),
                NabavkaVeznas = viewModel.Stavke.Select(s => new NabavkaVezna
                {
                    Opis = s.Opis,
                    Kolicina = s.Kolicina,
                    ArtiklId = s.ArtiklId,
                    StatusId = (int)Status.Na_čekanju,
                    Cijena = s.Cijena,
                    Dobavljac = s.Dobavljac
                }).ToList(),
                DatumiOdobravanjaNabavke = new DatumiOdobravanjaNabavke()
            };//nova nabavka

            trebovanjeNabavkeContext.Nabavkes.Add(novaNabavka);
            trebovanjeNabavkeContext.SaveChanges();

            var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name);
            OdobravanjeNabavkeController email = new OdobravanjeNabavkeController();
            if (narucilac.SektorSifra.ToLower() == "m" && viewModel.TipId == 2)
            {
                if (email.PosaljiMejlMarketinguZaNovuNabavku(novaNabavka.SerijskiBroj))
                {
                    novaNabavka.StatusNabavkeId = (int)Status.U_procesu_nabavke;
                    trebovanjeNabavkeContext.SaveChanges();
                    return Json(new { success = true });
                }                    
                else
                    return Json(new { success = false });
            }
            else
            {

                //Poziv metode koja salje meil marketingu sa linkom do nove nabavke
                string sadrzajMejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                                    adresaHosta + "OdobravanjeNabavke/OdobravanjeNabavke?nabavka=" + novaNabavka.NabavkaId + ">odobravanje nabavke</a>.<br/>" +
                                    "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                                    "Broj zahtjeva: " + novaNabavka.SerijskiBroj;

                sadrzajMejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";


                //mejl za novu nabavku ce poslati nadredjenom ako ga ima, ako ga nema poslat ce mejl direktno Timu prodaje i nabavki da imaju novu narudzbu
                //false vraca samo u slucaju da mejl nije poslat
                if (email.SlanjeMejlaZaNovuNabavku("Zahtjev za odobravanje nabavke", sadrzajMejla, narucilac.KorisnickoIme, serijskiBroj))
                    return Json(new { success = true });
                else
                    return Json(new { success = false });
            }
        }

        public ActionResult NovaNabavkaZaDaljuProdaju()
        {
            ViewBag.listaAtikala = trebovanjeNabavkeContext.vInformacijeOArtiklus.Select(t => new SelectListItem
            {
                Value = t.ArtikalId.ToString(),
                Text = t.Naziv

            }).ToList();
            ReferentiNabavke();
            Odgovorni();
            return View();
        }

        //Metoda za prikaz svih nabavki ulogovanog korisnika sa bilo kojim statusom nabavke
        [HandleError(View = "Error")]
        public ActionResult IstorijaNabavkiKorisnika(int? page)
        {
            List<NabavkaViewModel> ListaNabavki = ListaSvihNabavkiOdKorisnika();
            ViewBagStatusi();
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = ListaNabavki.ToPagedList(pageNumber, 6);
            return View("IstorijaNabavkiKorisnika", ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> ListaSvihNabavkiOdKorisnika()
        {
            string SifraRadnika = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name).RadnikSifra;

            List<Nabavke> DBListaNabavki = trebovanjeNabavkeContext.Nabavkes.Where(n => n.SifraRadnika == SifraRadnika).ToList();

            List<NabavkaViewModel> ListaNabavki = PopunjvanjeAtributaNabavke(DBListaNabavki);

            return ListaNabavki;
        }


        //Metoda za prikaz svih nabavki sektora za koji je ulogovani korisnik zadužen
        [HandleError(View = "Error")]
        [Authorize(Roles = "Tim lider,Menadzer,Skladistar,Referent skladista,Radnik za odobravanje, Generalni menadzer")]
        public ActionResult IstorijaNabavkiKorisnikaIzSektora(int? page)
        {


            List<NabavkaViewModel> ListaNabavki = ListaIstorijaNabavkiKorisnikaIzSektora();
            ViewBagStatusi();
            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = ListaNabavki.ToPagedList(pageNumber, 6);

            return View(ListaSaPaginacijom);
        }

        public List<NabavkaViewModel> ListaIstorijaNabavkiKorisnikaIzSektora()
        {
            string SifraRadnika = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name).RadnikSifra;

            List<Nabavke> DBListaNabavki = trebovanjeNabavkeContext.Nabavkes.Where(t => t.vRadnik.Nivo1OdobravanjaSifra == SifraRadnika ||
            t.vRadnik.Nivo2OdobravanjaSifra == SifraRadnika || t.vRadnik.Nivo3OdobravanjaSifra == SifraRadnika).ToList();

            List<NabavkaViewModel> ListaNabavki = PopunjvanjeAtributaNabavke(DBListaNabavki);

            return ListaNabavki;
        }

        private List<NabavkaViewModel> PopunjvanjeAtributaNabavke(List<Nabavke> DbNabavke)
        {
            List<NabavkaViewModel> ListaNabavki = DbNabavke.Select(n => new NabavkaViewModel
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
                    Spremno = (s.StatusId == (int)Status.Spremno_za_preuzimanje || s.StatusId == (int)Status.Preuzeto) ? true : false,
                    Preuzeto = s.StatusId == (int)Status.Preuzeto ? true : false,
                    Opis = s.Opis
                }).ToList()
            }).OrderByDescending(n => n.StatusNabavkeId).OrderByDescending(n => n.DatumPodnosenjaZahtjeva).ToList();

            return ListaNabavki;
        }

        [HandleError(View = "Error")]
        public ActionResult DetaljiNabavke(int nabavka)
        {
            var DbNabavka = trebovanjeNabavkeContext.Nabavkes.Where(t => t.NabavkaId == nabavka).ToList();
            NabavkaViewModel zahtjev = PopunjvanjeAtributaNabavke(DbNabavka).FirstOrDefault();
            return View(zahtjev);
        }

        //Metoda za generisanje novog serijskog broja na osnovu zadnjeg serijskog broja i trenutne godine
        private string NoviSerijskiBroj()
        {
            string zadnjiSerijskiBroj = "";
            var zadnjiSerijskiUBazi = trebovanjeNabavkeContext.Nabavkes.ToList().LastOrDefault();

            if (zadnjiSerijskiUBazi == null)
                zadnjiSerijskiBroj = "1/" + DateTime.Now.Year.ToString();
            else
            {
                zadnjiSerijskiBroj = zadnjiSerijskiUBazi.SerijskiBroj;
                //Provjera da li je godina zadnjeg serijskog broja jednaka trenutnoj godini
                if (zadnjiSerijskiBroj.Split('/')[1] == DateTime.Now.Year.ToString())
                {
                    //Ako je godina ista index se uveća za jedan
                    int indexSerijskogBroja = Convert.ToInt32(zadnjiSerijskiBroj.Split('/')[0]) + 1;
                    zadnjiSerijskiBroj = indexSerijskogBroja + "/" + DateTime.Now.Year.ToString();
                }
                else
                {
                    //Ako godina nije ista serijski broj ponovo kreće od 1 i dodaje se nova godina
                    zadnjiSerijskiBroj = "1/" + DateTime.Now.Year.ToString();
                }
            }
            return zadnjiSerijskiBroj;

           
           
        }

        //Metoda za Download dokumenata
        public FileResult PreuzimanjeDokumenta(int dokumentId)
        {
            var DbDokument = trebovanjeNabavkeContext.Dokuments.FirstOrDefault(x => x.DokumentId == dokumentId);
            byte[] fajl = DbDokument.Dokument1;

            return File(fajl, System.Net.Mime.MediaTypeNames.Application.Octet, DbDokument.Naziv);
        }

        [Authorize(Roles = "Referent nabavke")]
        [HandleError(View = "Error")]
        public ActionResult NarucivanjeNabavke(int nabavka)
        {
            var DbNabavka = trebovanjeNabavkeContext.Nabavkes.Where(o => o.NabavkaId == nabavka).ToList();
            
            NabavkaViewModel novaNabavka = PopunjvanjeAtributaNabavke(DbNabavka).FirstOrDefault();

            return View(novaNabavka);
        }
        
        //Metoda koja se poziva u skripti "SkriptaZaKreiranjeNoveNabavkeOsnovnogSredtsva.js" da sa zadatom sifrom artikla vrati njegovu cijenu
        public JsonResult VratiCijenuArtikla(int sifraArtikla)
        {
            decimal cijena = trebovanjeNabavkeContext.vInformacijeOArtiklus.FirstOrDefault(a => a.ArtikalId == sifraArtikla).Cijena;
            return Json(new { cijena }, JsonRequestBehavior.AllowGet);
        }

        #region Filter Za Nabavke
        //filter za sve navavke od ulogovanog korisnika
        public ActionResult FilterMojeNabavke(string SortiranjePoImenu, string SortiranjeDatum, int? page, string StatusTrebovanja)
        {

            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;
            ViewBag.StatusTrebovanja = StatusTrebovanja;

            //f-ja koja pravi viewBag statusi trebovanja za DropDownList na View-u
            //koristimo isi viewBag sa statusima za trebovanje
            ViewBagStatusi();

            List<NabavkaViewModel> listaAktivnihNabavki = ListaSvihNabavkiOdKorisnika();

            Helper.FilterNabavke filter = new Helper.FilterNabavke(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, listaAktivnihNabavki);


            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);

            return View("IstorijaNabavkiKorisnika", ListaSaPaginacijom);


        }
        public ActionResult FilterIstorijaNabavkiKorisnikaIzSektora(string SortiranjePoImenu, string SortiranjeDatum, int? page, string StatusTrebovanja)
        {
            ViewBag.SortiranjePoImenu = SortiranjePoImenu;
            ViewBag.SortiranjeDatum = SortiranjeDatum;
            ViewBag.StatusTrebovanja = StatusTrebovanja;

            //f-ja koja pravi viewBag statusi trebovanja za DropDownList na View-u
            //koristimo isi viewBag sa statusima za trebovanje
            ViewBagStatusi();

            List<NabavkaViewModel> lista = ListaIstorijaNabavkiKorisnikaIzSektora();

            Helper.FilterNabavke filter = new Helper.FilterNabavke(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, lista);


            var pageNumber = page ?? 1;
            var ListaSaPaginacijom = filter.Lista.ToPagedList(pageNumber, 6);

            return View("IstorijaNabavkiKorisnikaIzSektora", ListaSaPaginacijom);
        }

        public void ViewBagStatusi()
        {

            ViewBag.ListaStatusa = trebovanjeNabavkeContext.Status.Select(x => new SelectListItem
            {
                //Value = x.StatusId.ToString(),
                Text = x.StatusNaziv
            }).ToList();


        }
        #endregion
    }
}