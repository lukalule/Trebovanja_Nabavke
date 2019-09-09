using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;

namespace TrebovanjaNarudzbe.Web.Controllers
{
    public class EmailController : Controller
    {
        public string adresaHosta = @"http://localhost:51059/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;
        string naslov_Mejla = "";
        string sadrzaj_Mejla = "";
        string na_Mejl = "";
        bool poslano = false;

        public EmailController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }

        private List<TrebovanjeViewModel> PopunjavanjeAtributaTrebovanja(List<Trebovanje> DbTrebovanja)
        {
            List<TrebovanjeViewModel> VmTrebovanja = DbTrebovanja.Select(tr => new TrebovanjeViewModel
            {
                TrebovanjeId = tr.TrebovanjeId,
                NapomenaRadnika = tr.NapomenaRadnika,
                NapomenaNadredjenog=tr.NapomenaNadredjenog,
                DatumPodnesenogZahtjeva = tr.DatumPodnosenjaZahtjeva,
                SerijskiBroj = tr.SerijskiBroj,
                ImeIPrezimeRadnika = tr.vRadniks.Ime + " " + tr.vRadniks.Prezime,
                ListaArtikalaTrebovanja = tr.TrebovanjeVeznas.Select(art => new ArtiklViewModel
                {
                    ArtiklId = (int)art.ArtikalId,
                    Naziv = art.vInformacijeOArtiklu.Naziv,
                    TrebovanaKolicina = art.TrebovanaKolicina,
                    Cijena = art.vInformacijeOArtiklu.Cijena
                }).ToList()
            }).OrderByDescending(t => t.DatumPodnesenogZahtjeva).ToList();

            return VmTrebovanja;
        }

        #region Trebovanje
        public List<TrebovanjeViewModel> VratiListuTrebovanjaZaPregledOdobravanja(int statusTrebovanjaId, int nivoOdobravanja)
        {
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);

            List<Trebovanje> DbTrebovanja = new List<Trebovanje>();

            List<TrebovanjeViewModel> VmTrebovanja = new List<TrebovanjeViewModel>();
            // provjerava da je logovai radnik koji provjerava (odobrava) prvi nivo za odobravanja radnika koji je trebovao i da je status trebovanja na cekanju (prvi status nakon trebovanja)
            if (nivoOdobravanja == 1)
            {
                DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo1OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusTrebovanjaId == statusTrebovanjaId)).ToList();
                
                VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);
            }
            else if (nivoOdobravanja == 2)
            {
                DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo2OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusTrebovanjaId == statusTrebovanjaId)).ToList();

                VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);
            }
            else if (nivoOdobravanja == 3)
            {
                DbTrebovanja = trebovanjeNabavkeContext.Trebovanjes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo3OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusTrebovanjaId == statusTrebovanjaId)).ToList();
                
                VmTrebovanja = PopunjavanjeAtributaTrebovanja(DbTrebovanja);
            }

            return VmTrebovanja;
        }

        [Authorize(Roles = "Tim lider,Menadzer,Generalni menadzer,Radnik za odobravanje")]// provjerit da li treba autorizacija, zbog slucaja kad radnik ima pravo odobravnaja nekom
        public ActionResult OdobravanjeListeTrebovanja()
        {
            List<TrebovanjeViewModel> listaTrebovanja = new List<TrebovanjeViewModel>();
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            if (provjerivac.RadnikUlogaId == 1)
            {
                listaTrebovanja = VratiListuTrebovanjaZaPregledOdobravanja((int)Enum.Status.Na_čekanju, 1);
            }
            else if (provjerivac.RadnikUlogaId == 2)
            {
                listaTrebovanja = VratiListuTrebovanjaZaPregledOdobravanja((int)Enum.Status.Na_čekanju, 1);
            }
            else if (provjerivac.RadnikUlogaId == 3)
            {
                listaTrebovanja = VratiListuTrebovanjaZaPregledOdobravanja((int)Enum.Status.U_procesu_odobravanja, 2);
            }
            else if (provjerivac.RadnikUlogaId == 4)
            {
                listaTrebovanja = VratiListuTrebovanjaZaPregledOdobravanja((int)Enum.Status.U_procesu_odobravanja_viseg_nivoa, 3);
            }
            return View(listaTrebovanja);
        }

        [Authorize(Roles = "Tim lider,Menadzer,Generalni menadzer,Radnik za odobravanje")]
        public ActionResult OdobravanjeTrebovanja(int trebovanje)
        {
            TrebovanjeController trebovanjeController = new TrebovanjeController();
            var logovaniRadnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            var trebovanjeDB = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t => t.TrebovanjeId == trebovanje);
            List<Trebovanje> trebovanjeVM = trebovanjeNabavkeContext.Trebovanjes.Where(t => t.TrebovanjeId == trebovanje &&
            (logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo1OdobravanjaSifra
            || logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo2OdobravanjaSifra
            || logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo3OdobravanjaSifra)
            ).ToList();

            TrebovanjeViewModel VmTrebovanje = trebovanjeController.PopunjavanjeAtributaTrebovanja(trebovanjeVM).FirstOrDefault();

            if ((trebovanjeDB.StatusTrebovanjaId == (int)Enum.Status.Na_čekanju && logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo1OdobravanjaSifra)
                || (trebovanjeDB.StatusTrebovanjaId == (int)Enum.Status.U_procesu_odobravanja && logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo2OdobravanjaSifra)
                || (trebovanjeDB.StatusTrebovanjaId == (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa && logovaniRadnik.RadnikSifra == trebovanjeDB.vRadniks.Nivo3OdobravanjaSifra ))
                return View(VmTrebovanje);
            else if (VmTrebovanje == null)
            {
                ViewBag.Greska = "Link nije dostupan!";
                return View();
            }

            else
            {
                ViewBag.Greska = "Link nije aktivan!";
                return View();
            }
        }

        [HttpPost]
        public JsonResult SlanjeMejlaZaOdobravanjeTrebovanja(bool odobreno, int trebovanje, string napomena)
        {
            var narucilacSifra = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t => t.TrebovanjeId == trebovanje).SifraRadnika;
            var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.RadnikSifra == narucilacSifra);
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            var trebovanjeDB = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(tr => tr.TrebovanjeId == trebovanje);
            trebovanjeDB.NapomenaNadredjenog = napomena;
            if (trebovanjeDB == null)//dodati jos i ako je trebovanje odobreno da ga ne moze vise otvorit pod "odobravanjem"
                return Json(new { succses = false, message = "Trebovanje ne postoji!" });
            if (provjerivac.RadnikSifra == narucilac.Nivo1OdobravanjaSifra)//provjera da li je logovani prvi nivo za odobravnja
            {
                // ako nema nadredjenog viseg nivoa, a odobreno je, salji u skladiste ili marketing
                if (narucilac.Nivo2OdobravanjaSifra == null && narucilac.Nivo3OdobravanjaSifra == null && odobreno)
                {
                    trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa1 = DateTime.Now;
                    trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odobreno;//status odobreno
                    var rezervisaniArtikli = trebovanjeNabavkeContext.RezervisaniArtiklis;
                    //dodaje trebovane artikle i njihovu kolicinu na rezervaciju na rezervaciju
                    foreach (var artiklRezervisanDB in rezervisaniArtikli)
                    {
                        foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                        {
                            if (artiklRezervisanDB.ArtikalId == trebovaniArtikl.ArtikalId)
                            {

                                artiklRezervisanDB.RezervisanaKolicina += trebovaniArtikl.TrebovanaKolicina;
                            }
                        }
                    }
                    if (SlatiUSkladiste(trebovanje))
                    {
                        poslano = PosaljiMejlSkladistaruZaTrebvanje(narucilacSifra, trebovanjeDB.TrebovanjeId);
                        //salje mejl  i naruciocu da je narudzba odobrena i da su artikli u pripremi
                        if (poslano)
                        {


                            poslano = false;
                            naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen u skladište. Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje </p><br/><br/> ";
                            sadrzaj_Mejla += "<span>Srdačan pozdrav, <br/><br/>Lanaco trebovanje i nabavke</span>";
                            na_Mejl = narucilac.Email;
                            poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                            if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                            {
                                foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                                {
                                    trebovaniArtikl.StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi; // status artikl u pripremi
                                }

                                trebovanjeNabavkeContext.SaveChanges();
                                return Json(new { succses = true });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }
                    // nema na stanju nekog artikla salji trebovanje mejlom  u marketing  da se naruci
                    else
                    {
                        poslano = PosaljiMejlReferentuSkladistaUMarketingu(trebovanje);
                        trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                                                              //salje mejl  i naruciocu da je narudzba odobrena i da je otisla u marketing radi narudžbe artikala
                        if (poslano)
                        {
                            poslano = false;
                            naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                 trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen Timu prodaje i nabavke, radi nabavke nekih od stavki kojih nema na stanju.<br/>" +
                                 "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p><br/><br/> ";
                            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = narucilac.Email;
                            poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                            if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                            {
                                trebovanjeNabavkeContext.SaveChanges();
                                return Json(new { succses = true });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }

                }
                else
                {
                    if (odobreno)
                    {
                        vRadnik nadredjeni = new vRadnik();
                        //salje menadzeru (sefu) dalje
                        if (narucilac.Nivo2OdobravanjaSifra != null)
                        {
                            var timLider = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo1OdobravanjaSifra);
                            nadredjeni = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo2OdobravanjaSifra);
                            naslov_Mejla = "Zahtjev za odobravanje trebovanja";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                                adresaHosta + "Email/OdobravanjeTrebovanja?trebovanje=" + trebovanjeDB.TrebovanjeId + ">odobravanje trebovanja</a>. <br/>" +
                                "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                                "Broj zahtjeva: " + trebovanjeDB.SerijskiBroj;

                            sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = nadredjeni.Email;
                            trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_odobravanja; // status U procesu odobravanja
                            trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa1 = DateTime.Now;
                        }
                        else if (narucilac.Nivo3OdobravanjaSifra != null)
                        {
                            nadredjeni = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo3OdobravanjaSifra);
                            naslov_Mejla = "Zahtjev za odobravanje trebovanja";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                               adresaHosta + "Email/OdobravanjeTrebovanja?trebovanje=" + trebovanjeDB.TrebovanjeId + ">odobravanje trebovanja</a>. <br/>" +
                               "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                               "Broj zahtjeva: " + trebovanjeDB.SerijskiBroj;

                            sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = nadredjeni.Email;
                            trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa;  
                            trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa1 = DateTime.Now;
                        }
                        else
                        {//nema nadredjenog salji u skladiste

                            trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odobreno;//status odobreno
                                                                                                     //Pregledati dodjelu statusa  3 i 1007 na zadnjem testiranju nije upisalo u bazu
                                                                                                     //dodaje trebovane artikle i njihovu kolicinu na rezervaciju na rezervaciju
                            trebovanjeDB.TrebovanjeVeznas.Select(t => t.RezervisaniArtikli.RezervisanaKolicina += t.TrebovanaKolicina);
                            if (SlatiUSkladiste(trebovanje))
                            {
                                poslano = PosaljiMejlSkladistaruZaTrebvanje(narucilacSifra, trebovanjeDB.TrebovanjeId);

                                if (poslano)
                                {
                                    trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odobreno;// status odobreno
                                                                      //salje mejl  i naruciocu da je narudzba odobrena
                                    poslano = false;
                                    naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                                    sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                        trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen u skladište. Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje </p><br/><br/> ";
                                    sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                                    na_Mejl = narucilac.Email;
                                    poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                                    if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                                    {
                                        foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                                        {
                                            trebovaniArtikl.StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi; // status artikl u pripremi
                                        }
                                        trebovanjeNabavkeContext.SaveChanges();
                                        return Json(new { succses = true });
                                    }
                                    else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                        return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                                }
                                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                            }
                            else// nema na stanju nekog artikla salji trebovanje mejlom  u marketing  da se naruci
                            {

                                poslano = PosaljiMejlReferentuSkladistaUMarketingu(trebovanje);
                                trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                                if (poslano)
                                {
                                    //salje mejl  i naruciocu da je narudzba odobrena
                                    poslano = false;
                                    naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                                    sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                         trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen Timu prodaje i nabavke, radi nabavke nekih od stavki kojih nema na stanju.<br/>" +
                                         "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p><br/><br/> ";
                                    sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                                    na_Mejl = narucilac.Email;
                                    poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

                                    if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                                    {
                                        trebovanjeNabavkeContext.SaveChanges();
                                        return Json(new { succses = true });
                                    }
                                    else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                        return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                                }
                                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                            }
                        }
                    }
                    else// trebovanje odbijeno od strane tim lidela
                    {
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                  trebovanje + ">trebovanje </a> je odbijeno</p><br/> <br/>";
                        sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                        na_Mejl = narucilac.Email;
                        trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odbijeno;
                        trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa1 = DateTime.Now;
                    }
                    poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);// salje za slucaj da je odbijeno trebovanje 

                    if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                    {
                        trebovanjeNabavkeContext.SaveChanges(); return Json(new { succses = true });
                    }
                    else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                        return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                }
            }
            else if (provjerivac.RadnikSifra == narucilac.Nivo2OdobravanjaSifra)
            {
                if (odobreno)
                {
                    trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa2 = DateTime.Now;
                    if (narucilac.Nivo3OdobravanjaSifra != null)// ako postoji nivo 3 odobravanja salji njemu mejl i redirektuj
                    {
                        var nadredjeni = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo3OdobravanjaSifra);
                        naslov_Mejla = "Zahtjev za odobravanje trebovanja";
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                               adresaHosta + "Email/OdobravanjeTrebovanja?trebovanje=" + trebovanjeDB.TrebovanjeId + ">odobrabanje trebovanja</a>. <br/>" +
                               "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                               "Broj zahtjeva: " + trebovanjeDB.SerijskiBroj;

                        sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        na_Mejl = nadredjeni.Email;
                        poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                        if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                        {
                            trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa;
                            trebovanjeNabavkeContext.SaveChanges();
                            return Json(new { succses = true });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }
                    else if (narucilac.Nivo3OdobravanjaSifra == null)
                    {
                        trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odobreno;//status odobreno
                        var rezervisaniArtikli = trebovanjeNabavkeContext.RezervisaniArtiklis;//.Where(a=> trebovanje.TrebovanjeVeznas.Where(tv=>tv.ArtikalId==a.ArtikalId))
                        foreach (var artiklRezervisanDB in rezervisaniArtikli)
                        {
                            foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                            {
                                if (artiklRezervisanDB.ArtikalId == trebovaniArtikl.ArtikalId)
                                {
                                    artiklRezervisanDB.RezervisanaKolicina += trebovaniArtikl.TrebovanaKolicina;//dodaje trebovane artikle i njihovu kolicinu na rezervaciju 
                                }
                            }
                        }
                        if (SlatiUSkladiste(trebovanje))
                        {
                            poslano = PosaljiMejlSkladistaruZaTrebvanje(narucilacSifra, trebovanjeDB.TrebovanjeId);
                            if (poslano)
                            {

                                //salje mejl  i naruciocu da je narudzba odobrena i da je u skladistu
                                poslano = false;
                                naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                                sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                    trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen u skladište. Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p><br/><br/> ";
                                sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                                na_Mejl = narucilac.Email;
                                poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                                if (poslano) //AKO JE USPJESNO POSLALO dodjeljuje trebovanim artiklima status da su u pripremi 
                                {
                                    foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                                    {
                                        trebovaniArtikl.StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi; // status artikl u pripremi
                                    }
                                    trebovanjeNabavkeContext.SaveChanges();
                                    return Json(new { succses = true });
                                }
                                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                        else// nema na stanju nekog artikla salji trebovanje mejlom  u marketing  da se naruci
                        {
                            poslano = PosaljiMejlReferentuSkladistaUMarketingu(trebovanje);
                            trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                            if (poslano)
                            {
                                //salje mejl  i naruciocu da je narudzba odobrena
                                poslano = false;
                                naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                                sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                     trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen Timu prodaje i nabavke, radi nabavke nekih od stavki kojih nema na stanju.<br/>" +
                                     "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p><br/><br/> ";
                                sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                                na_Mejl = narucilac.Email;
                                poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

                                if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                                {
                                    trebovanjeNabavkeContext.SaveChanges();
                                    return Json(new { succses = true });
                                }
                                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                    }

                }
                else // trebovanje odbijeno od strane sefa ili gen.men.
                {
                    sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                   trebovanje + ">trebovanje </a> je odbijeno</p><br/> <br/>";
                    sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                    naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                    na_Mejl = narucilac.Email;
                    trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odbijeno;
                    trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa2 = DateTime.Now;
                }
                poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla); // salje za slucaj da je odbijeno trebovanje 

                if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                {
                    trebovanjeNabavkeContext.SaveChanges();
                    return Json(new { succses = true });
                }
                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
            }

            else if (provjerivac.RadnikSifra == narucilac.Nivo3OdobravanjaSifra)
            {
                if (odobreno)
                {
                    trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa3 = DateTime.Now;
                    trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odobreno;
                    var artikli = trebovanjeNabavkeContext.RezervisaniArtiklis;
                    foreach (var artiklRezervisanDB in artikli)
                    {
                        foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                        {
                            if (artiklRezervisanDB.ArtikalId == trebovaniArtikl.ArtikalId)
                            {
                                artiklRezervisanDB.RezervisanaKolicina += trebovaniArtikl.TrebovanaKolicina;
                            }
                        }
                    }
                    if (SlatiUSkladiste(trebovanje))
                    {

                        poslano = PosaljiMejlSkladistaruZaTrebvanje(narucilacSifra, trebovanjeDB.TrebovanjeId);

                        if (poslano)
                        {// salje mejl naruciocu da je trebovanje odobreno i da je yahtjev poslan u skladiste
                            poslano = false;
                            naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen u skladište. Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje </p> <br/><br/>";
                            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = narucilac.Email;
                            poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                            if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                            {
                                foreach (var trebovaniArtikl in trebovanjeDB.TrebovanjeVeznas)
                                {
                                    trebovaniArtikl.StatusArtiklaId = (int)Enum.Status.Artikl_u_pripremi; // status artikl u pripremi
                                }
                                trebovanjeNabavkeContext.SaveChanges();
                                return Json(new { succses = true });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                        else Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }
                    else
                    {// nema na skladistu dovoljno salji u marketing

                        poslano = PosaljiMejlReferentuSkladistaUMarketingu(trebovanje);
                        trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                        if (poslano)
                        {
                            //salje mejl  i naruciocu da je narudzba odobrena i da je otisla u marketing radi narudžbe nekih artikala
                            poslano = false;
                            naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                 trebovanje + ">trebovanje </a> je odobreno, zahtjev je proslijeđen u Timu prodaje i nabavki zbog nabavke nekih od stavki kojih nema na stanju.<br/>" +
                                 "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p> <br/><br/>";
                            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = narucilac.Email;
                            poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

                            if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                            {
                                trebovanjeNabavkeContext.SaveChanges();
                                return Json(new { succses = true });
                            }
                            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }
                }
                else // trebovanje odbijeno od strane sefa ili gen.men.
                {
                    sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Trebovanje/DetaljiTrebovanja?trebovanje=" +
                                  trebovanje + ">trebovanje </a> je odbijeno</p><br/> <br/>";
                    sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                    naslov_Mejla = "Odgovor na zahtjev za trebovanje";
                    na_Mejl = narucilac.Email;
                    trebovanjeDB.StatusTrebovanjaId = (int)Enum.Status.Odbijeno;
                    trebovanjeDB.DatumiOdobravanjaTrebovanje.DatumOdobravanjaNivoa3 = DateTime.Now;
                }
                poslano = PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);// salje za slucaj da je odbijeno trebovanje 

                if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                {
                    trebovanjeNabavkeContext.SaveChanges();
                    return Json(new { succses = true });
                }
                else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                    return Json(new { succses = false, message = "Neuspješno slanje mejla" });
            }

            else
                return Json(new { succses = false, message = "Logovana osoba nema ovlaštenja da odobrava ovo trebovanje!" });

        }

        public bool PosaljiMejl(string naslov, string naMejl, string sadrzajMejla)
        {
            var emailPoruka = new MailMessage
            {
                IsBodyHtml = true,
                Subject = naslov,
                Body = sadrzajMejla
            };
            emailPoruka.To.Add(naMejl);
            using (var smtp = new SmtpClient())
            {
                try
                {
                    smtp.Send(emailPoruka);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        //pozvati kad marketing sacuva izmjene na nabavci, provjeriti ko je nadredjeni za radnika koji 
        //je napravio nabavku i onda njega postaviti da bude logovani radnik
        //nakon toga bi cijela procedura trebala da bude ista ako je ista struktura odobravanja
        public bool SlanjeMejlaZaNovoTrebovanje(string naslov, string sadrzaj, string korisnickoImeLogovanog, string serijskiBrojTrebovanja)//prepraviti
        {
            bool poslano = false;
            //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var emailPoruka = new MailMessage
            {
                Subject = naslov,
                IsBodyHtml = true,
                Body = sadrzaj
            };
            // logovani korisnik = korisnik koji vrsi trebovanje 
            var logovaniKorisnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == korisnickoImeLogovanog);
            var trebovanje = trebovanjeNabavkeContext.Trebovanjes.FirstOrDefault(t => t.SerijskiBroj == serijskiBrojTrebovanja);
            var mejlNadredjeni = "";

            if (logovaniKorisnik.Nivo1OdobravanjaSifra != null)
            {
                trebovanje.StatusTrebovanjaId = (int)Enum.Status.Na_čekanju;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo1OdobravanjaSifra).Email;
            }
            else if (logovaniKorisnik.Nivo2OdobravanjaSifra != null)
            {
                trebovanje.StatusTrebovanjaId = (int)Enum.Status.U_procesu_odobravanja;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo2OdobravanjaSifra).Email;
            }
            else if (logovaniKorisnik.Nivo3OdobravanjaSifra != null)
            {
                trebovanje.StatusTrebovanjaId = (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo3OdobravanjaSifra).Email;
            }

            if (mejlNadredjeni != "")
                emailPoruka.To.Add(mejlNadredjeni);
            else return false;
            using (var smtp = new SmtpClient())
            {
                smtp.Send(emailPoruka);
                poslano = true;
                trebovanjeNabavkeContext.SaveChanges();
            }
            return poslano;
        }

        public bool SlatiUSkladiste(int idTrebovanja)
        {
            MarketingController marketing = new MarketingController();
            List<ArtiklViewModel> artikli = marketing.ProvjeraKolicineStanja(idTrebovanja);
            return !artikli.Any(a => a.NedostajucaKolicna > 0);
        }
        public bool PosaljiMejlSkladistaruZaTrebvanje(string sifraNarucioca, int idTrebovanja)
        {
            var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.RadnikSifra == sifraNarucioca);
            var skladistar = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(r => r.SektorSifra == "S" && r.RadnikUlogaId == 7);
            naslov_Mejla = "Zahtjev za trebovanje";//SKLADISTARU SE SALJE LINK DO DETALJA NARUDBE KOJU TREBA NAPRAVIT (NEMAMO JOS TAJ VIEW!!!)
            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Na sistemu LANACO trebovanje i nabavke je novo <a href=" + adresaHosta + "Skladiste/PrikazJednogTrebovanja?trebovanje=" +
                                idTrebovanja + ">trebovanje </a> koje je odobreno radniku " + narucilac.Ime + " " + narucilac.Prezime + " iz sektora " + narucilac.NazivSektora + ".</p> <br/><br/>";
            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
            na_Mejl = skladistar.Email;
            return PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
        }
        public bool PosaljiMejlReferentuSkladistaUMarketingu(int idTrebovanja)
        {
            var marketing = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(r => r.SektorSifra == "M" && r.RadnikUlogaId == 5);
            poslano = false;
            naslov_Mejla = "Zahtjev za trebovanje";
            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Na sistemu LANACO trebovanje i nabavke je novo <a href=" + adresaHosta + "Marketing/NovoTrebovanjeMarketing?trebovanje=" +
                                 idTrebovanja + ">trebovanje </a> za koje je potrebno naručiti određene artikle.</p> <br/><br/>";
            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
            na_Mejl = marketing.Email;
            return PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
        }
        #endregion


    }
}