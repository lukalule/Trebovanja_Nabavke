using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;
using TrebovanjaNarudzbe.Web.Enum;
using X.PagedList;
using System.Linq.Dynamic;


namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class TrebovanjeController : Controller
    {
        readonly string adresaHosta = @"http://localhost:51059/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;

        public TrebovanjeController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }



        //Metoda za prikaz detalja jednog trebovanja
        [HandleError(View = "Error")]
        public ActionResult DetaljiTrebovanja(int trebovanje)
        {
            var trebovanjeVM = TrazenoTrebovanje(trebovanje);
            if (trebovanjeVM == null)
                return View("Error");
            else
                return View(trebovanjeVM);
        }

        //Metoda za prikaz svih trebovanja ulogovanog korisnika sa bilo kojim statusom trebovanja
        [HandleError(View = "Error")]
        public ActionResult IstorijaTrebovanjaKorisnika(int? page, string vrijednost)
        {
            ViewBag.Metoda = "IstorijaTrebovanjaKorisnika";
            ViewBag.Naslov = "Moja trebovanja";
            //Kupljenje šifre ulogovanog radnika
            string sifraRadnika = SifraLogovanogRadnika();

            //Lista svih trebovanja ulogovanog korisnika
            var DbTrebovanje = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.SifraRadnika == sifraRadnika).ToList();
            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanje);

            return Paginacija(VmTrebovanja, page, vrijednost);
        }


        //Metoda za prikaz svih trebovanja sektora za koji je ulogovani korisnik zadužen
        [HandleError(View = "Error")]
        [Authorize(Roles = "Tim lider,Menadzer,Skladistar,Referent skladista,Radnik za odobravanje,Generalni menadzer")]
        public ActionResult IstorijaTrebovanjaKorisnikaIzSektora(int? page, string vrijednost)
        {
            ViewBag.Naslov = "Istorija trebovanja korisnika mog sektora";
            ViewBag.Metoda = "IstorijaTrebovanjaKorisnikaIzSektora";
            //Kupljenje šifre ulogovanog radnika
            string sifraRadnika = SifraLogovanogRadnika();

            //Lista svih trebovanja ulogovanog korisnika koje je njegov sektor obavio
            var DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(t => (t.vRadniks.Nivo1OdobravanjaSifra == sifraRadnika ||
            t.vRadniks.Nivo2OdobravanjaSifra == sifraRadnika || t.vRadniks.Nivo3OdobravanjaSifra == sifraRadnika)).ToList();

            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja).OrderBy(t => t.DatumPodnesenogZahtjeva).Reverse().ToList();

            return Paginacija(VmTrebovanja, page, vrijednost);
        }

        public ActionResult Paginacija(List<TrebovanjeViewModel> VmTrebovanja, int? page, string vrijednost)
        {
            //provjera ako je pocetna vrijednost null da odmah popuni pnput za filter
            if (vrijednost == null)
                vrijednost = "2";

            //filter od najnovijeg ka najstarijem ne treba jer je lista vec tako sortirana 
            if (vrijednost == "1" || vrijednost == "2")
            {
                VmTrebovanja = FilterPoDatumu(vrijednost, VmTrebovanja);
            }

            var pageNumber = page ?? 1;

            // if no page was specified in the querystring, default to the first page (1)// will only contain 25 products max because of the pageSize
            return View("ListaTrebovanja", VmTrebovanja.ToPagedList(pageNumber, 6));
        }

        public List<TrebovanjeViewModel> FilterPoDatumu(string vrijednost, List<TrebovanjeViewModel> listaTrebovanja)
        {
            //filter od najstarijeg ka najnovijem  == 1
            //filter od najnovijeg ka najstarijem ==  2 
            switch (vrijednost)
            {
                case "1":
                    listaTrebovanja = listaTrebovanja.OrderBy(o => o.DatumPodnesenogZahtjeva).ToList();
                    ViewBag.SortiranjeDatum = "Od najstarijeg ka najnovijem";
                    break;
                case "2":
                    ViewBag.SortiranjeDatum = "Od najnovijeg ka najstarijem";
                    break;
            }
            return listaTrebovanja;

        }

        //Kupljenje šifre ulogovanog radnika
        public string SifraLogovanogRadnika()
        {
            string sifraRadnika = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name).RadnikSifra;
            return sifraRadnika;
        }

        public List<TrebovanjeViewModel> PopunjavanjeAtributaTrebovanja(List<Trebovanje> DbTrebovanja)
        {
            MarketingController marketingController = new MarketingController();
            List<TrebovanjeViewModel> listaTrebovanja = DbTrebovanja.Select(t => new TrebovanjeViewModel
            {
                TrebovanjeId = t.TrebovanjeId,
                SerijskiBroj = t.SerijskiBroj,
                NapomenaRadnika = (t.NapomenaRadnika != null) ? (t.NapomenaRadnika.Contains("/n") ? t.NapomenaRadnika.Replace("/n", "\n") : t.NapomenaRadnika) : "",
                NapomenaNadredjenog = (t.NapomenaNadredjenog != null) ? (t.NapomenaNadredjenog.Contains("/n") ? t.NapomenaNadredjenog.Replace("/n", "\n") : t.NapomenaNadredjenog) : "",
                DatumPodnesenogZahtjeva = t.DatumPodnosenjaZahtjeva,
                DatumZaduzenjaTrebovanja = t.DatumZaduzenjaTrebovanja,
                SifraRadnika = t.SifraRadnika,
                NazivStatusa = t.Status.StatusNaziv,
                ImeIPrezimeRadnika = t.vRadniks.Ime + " " + t.vRadniks.Prezime,
                ListaArtikalaTrebovanja = marketingController.ProvjeraKolicineStanja(t.TrebovanjeId)
            }).OrderByDescending(t => t.DatumPodnesenogZahtjeva).ToList();

            return listaTrebovanja;
        }

        //Metoda za vraćanje jednog trebovanja u zavisnosti id-a
        [HandleError(View = "Error")]
        public TrebovanjeViewModel TrazenoTrebovanje(int trebovanje)
        {
            var DbTrebovanje = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.TrebovanjeId == trebovanje).ToList();

            TrebovanjeViewModel VmTrebovanje = PopunjavanjeAtributaTrebovanja(DbTrebovanje).FirstOrDefault();

            return VmTrebovanje;
        }

        #region Kreiranje novog trebovanja
        [HandleError(View = "Error")]
        public ActionResult KreiranjeNovogTrebovanja()
        {
            //Lista artikala za popunjavanje dropdown-a sa svim artiklima
            ViewBag.listaAtikala = trebovanjeNabavkeContext.vInformacijeOArtiklus.Select(t => new SelectListItem
            {
                Value = t.ArtikalId.ToString(),
                Text = t.Naziv
            }).ToList();

            return View(new TrebovanjeViewModel());
        }

        JsonResult ProvjeraPostojanjaSvihArtikala(List<ArtiklViewModel> artikls)
        {
            bool postojeSvi = trebovanjeNabavkeContext.vInformacijeOArtiklus.ToList().Any(x => artikls.All(z => z.ArtiklId == x.ArtikalId));

            return Json(new { succses = postojeSvi });
        }

        [HttpPost]
        [HandleError(View = "Error")]
        public JsonResult KreiranjeNovogTrebovanja(List<ArtiklViewModel> artikliZaTrebovanje, string napomenaRadnika)
        {

            //Dio koda za provjeravanje da li se svi primljeni artikli u listi view modela nalaze unutar baze podataka za artikle
            bool provjeraPostojanjaSvihArtikala = trebovanjeNabavkeContext.vInformacijeOArtiklus.ToList().Any(x =>
            artikliZaTrebovanje.Any(z => z.ArtiklId == x.ArtikalId));

            if (provjeraPostojanjaSvihArtikala)
            {
                //Dodavanje novog trebovanja u bazu
                string noviSerijskiBroj = NoviSerijskiBroj();
                var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(x => x.KorisnickoIme == User.Identity.Name);
                var novoTrebovanje = new Trebovanje
                {
                    SifraRadnika = narucilac.RadnikSifra,
                    SerijskiBroj = noviSerijskiBroj,
                    DatumPodnosenjaZahtjeva = DateTime.Now,
                    StatusTrebovanjaId = (int)Enum.Status.Na_čekanju,
                    NapomenaRadnika = napomenaRadnika.Replace("\n", "/n"),
                    TrebovanjeVeznas = artikliZaTrebovanje.Select(x => new TrebovanjeVezna
                    {
                        ArtikalId = x.ArtiklId,
                        TrebovanaKolicina = x.TrebovanaKolicina,
                        KolicinaKojaNedostaje = 0,
                        StatusArtiklaId = (int)Enum.Status.Na_čekanju,
                        RezervisaniArtikli = trebovanjeNabavkeContext.RezervisaniArtiklis.FirstOrDefault(a => a.ArtikalId == x.ArtiklId)
                    }).ToList(),
                    DatumiOdobravanjaTrebovanje = new DatumiOdobravanjaTrebovanje()
                };
                trebovanjeNabavkeContext.Trebovanjes.Add(novoTrebovanje);
                trebovanjeNabavkeContext.SaveChanges();

                foreach (var artikl in novoTrebovanje.TrebovanjeVeznas)
                {
                    artikl.KolicinaKojaNedostaje = ((artikl.RezervisaniArtikli.TrenutnaKolicina - artikl.RezervisaniArtikli.RezervisanaKolicina) >= artikl.TrebovanaKolicina) ?//ako na stanju ima dovoljno za moju rezervaciju vrati 0 za narucit
                        0 : ((artikl.RezervisaniArtikli.TrenutnaKolicina <= artikl.RezervisaniArtikli.RezervisanaKolicina) ?
                        artikl.TrebovanaKolicina : (artikl.TrebovanaKolicina - (artikl.RezervisaniArtikli.TrenutnaKolicina - artikl.RezervisaniArtikli.RezervisanaKolicina)));
                    //ako je rezervisana vec cijela kolicina na stanju ili vise od nje vrati mi cijelu moju kolicinu za trebovanje da se naruci
                    //ako trebujem vise nego sto je ostalo na stanju vrati mi da treba naruciti razliku izmedju dostupne kolicine i moje trebovane kolicine
                    
                }
                trebovanjeNabavkeContext.SaveChanges();
                //Slanje mejla nadređenom sa linkom kreiranog trebovanja
                string sadrzajMejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                                adresaHosta + "Email/OdobravanjeTrebovanja?trebovanje=" + novoTrebovanje.TrebovanjeId + ">odobravanje trebovanja</a>. <br/>" +
                                "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                                "Broj zahtjeva: " + novoTrebovanje.SerijskiBroj;

                sadrzajMejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                EmailController email = new EmailController();
                if (email.SlanjeMejlaZaNovoTrebovanje("Zahtjev za odobravanje trebovanja", sadrzajMejla, narucilac.KorisnickoIme, novoTrebovanje.SerijskiBroj))
                {
                    trebovanjeNabavkeContext.SaveChanges();
                    return Json(new { succses = true });
                }
                else
                {// slanje mejla za novo trebovanje vraca false ako narucioc nema ni jednog nivoa odobravnja i u tom slucaju zahtjev se salje u skladiste
                    bool poslano = false;
                    string naslovMejla = "Odgovor na zahtjev za trebovanje";
                    string naMejl = "";
                    if (email.SlatiUSkladiste(novoTrebovanje.TrebovanjeId))
                    {
                        sadrzajMejla = "<p>Poštovani, <br/><br/> Vaš <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                            novoTrebovanje.TrebovanjeId + "> zahtjev za trebovanje </a>  je proslijeđen u skladište. Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje </p> <br/><br/>";
                        sadrzajMejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        naMejl = narucilac.Email;
                        poslano = email.PosaljiMejl(naslovMejla, naMejl, sadrzajMejla);
                        email.PosaljiMejlSkladistaruZaTrebvanje(narucilac.RadnikSifra, novoTrebovanje.TrebovanjeId);
                        novoTrebovanje.StatusTrebovanjaId = (int)Enum.Status.Odobreno;
                        foreach (var artikl in novoTrebovanje.TrebovanjeVeznas)
                        {
                            artikl.StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi;
                        }
                        trebovanjeNabavkeContext.SaveChanges();
                        return Json(new { succses = true, message = "Mejl poslan u skladište, jer naručilac nema nadredjenog." });
                    }
                    else
                    {

                        sadrzajMejla = "<p>Poštovani, <br/><br/> Vaš <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                             novoTrebovanje.TrebovanjeId + ">zahtjev za trebovanje </a> je proslijeđen u Timu prodaje i nabavki zbog nabavke nekih od stavki kojih nema na stanju.<br/>" +
                             "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p> <br/><br/>";
                        sadrzajMejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        naMejl = narucilac.Email;
                        poslano = email.PosaljiMejl(naslovMejla, naMejl, sadrzajMejla);
                        if (poslano)
                        {
                            email.PosaljiMejlReferentuSkladistaUMarketingu(novoTrebovanje.TrebovanjeId);
                            novoTrebovanje.StatusTrebovanjaId = (int)Enum.Status.U_procesu_nabavke;
                            return Json(new { succses = true, message = "Mejl poslan u skladište, jer naručilac nema nadredjenog." });
                        }
                        else
                        {
                            return Json(new { succses = false, message = "Neuspjesno slanje mejla" });
                        }
                    }
                }
            }
            else
            {
                //Vratiti Json da jedan od artikala se nalazi u bazi artikala
                return Json(new { succses = false });
            }
        }

        //Metoda za generisanje novog serijskog broja na osnovu zadnjeg serijskog broja i trenutne godine
        private string NoviSerijskiBroj()
        {
            string zadnjiSerijskiBroj = "";
            var zadnjiSerijskiUBazi = trebovanjeNabavkeContext.Trebovanjes.ToList().LastOrDefault();
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
        #endregion

        #region DataTablesClientSide 
        public ActionResult TabelarniPrikazIstorijaTrebovanjaKorisnika()
        {
            return View();
        }

        public ActionResult TabelarniPrikazIstorijaTrebovanjaKorisnikaIzSektora()
        {
            return View();
        }

        //json lista za logovanog korisnika
        public ActionResult ListaTrebovanjaLogovanogKorisnika()
        {
            string sifraRadnika = SifraLogovanogRadnika();       

            //Lista svih trebovanja ulogovanog korisnika
            var listaTrebovanjaKorisnika = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.SifraRadnika == sifraRadnika).ToList();
            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(listaTrebovanjaKorisnika);

            return Json(new { data = VmTrebovanja }, JsonRequestBehavior.AllowGet);
        }

        //json lista za sve korisnike u sektoru
        public ActionResult ListaTrebovanjaKorisnikaIzSektora()
        {
            string sifraRadnika = SifraLogovanogRadnika();

            //Lista svih trebovanja ulogovanog korisnika
            var listaKorisnikaIzSektora = trebovanjeNabavkeContext.Trebovanjes.Where(t => (t.vRadniks.Nivo1OdobravanjaSifra == sifraRadnika ||
            t.vRadniks.Nivo2OdobravanjaSifra == sifraRadnika || t.vRadniks.Nivo3OdobravanjaSifra == sifraRadnika)).ToList();

            List<TrebovanjeViewModel> VmTrebovanja = PopunjavanjeAtributaTrebovanja(listaKorisnikaIzSektora).OrderBy(t => t.DatumPodnesenogZahtjeva).Reverse().ToList();

            return Json(new { data = VmTrebovanja }, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}



