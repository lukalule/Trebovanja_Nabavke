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
    public class OdobravanjeNabavkeController : Controller
    {
        public string adresaHosta = @"http://localhost:51059/";
        private readonly TrebovanjeNabavkeContext trebovanjeNabavkeContext;
        EmailController email = new EmailController();

        string naslov_Mejla = "";
        string sadrzaj_Mejla = "";
        string na_Mejl = "";
        bool poslano = false;

        public OdobravanjeNabavkeController()
        {
            trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
        }


        // GET: OdobravanjeNabavke
        [Authorize(Roles = "Tim lider,Menadzer,Generalni menadzer,Radnik za odobravanje")]
        public ActionResult OdobravanjeNabavke(int nabavka)
        {
            var logovaniRadnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            var nabavkaDB = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.NabavkaId == nabavka);

            List<Nabavke> DbNabavke = trebovanjeNabavkeContext.Nabavkes.Where(t => t.NabavkaId == nabavka &&
            (logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo1OdobravanjaSifra
            || logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo2OdobravanjaSifra
            || logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo3OdobravanjaSifra)
            ).ToList();

            NabavkaViewModel nabavkaVM = PopunjavanjeAtributa(DbNabavke).FirstOrDefault();

            if ((nabavkaDB.StatusNabavkeId == (int)Enum.Status.Na_čekanju && logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo1OdobravanjaSifra)
                || (nabavkaDB.StatusNabavkeId == (int)Enum.Status.U_procesu_odobravanja && logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo2OdobravanjaSifra)
                || (nabavkaDB.StatusNabavkeId == (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa && logovaniRadnik.RadnikSifra == nabavkaDB.vRadnik.Nivo3OdobravanjaSifra))
                return View(nabavkaVM);

            else if (nabavkaVM == null)
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

        [HttpPost]//promjeniti naziv metode
        public JsonResult SlanjeMejlaZaOdobravanjeNabavke(bool odobreno, int nabavka, string napomena)
        {
            var narucilacSifra = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.NabavkaId == nabavka).SifraRadnika;
            var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.RadnikSifra == narucilacSifra);
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            var nabavkaDB = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(tr => tr.NabavkaId == nabavka);
            nabavkaDB.NapomenaSefa = napomena;
            if (nabavkaDB == null)//dodati jos i ako je nabavka odobrena da je ne moze vise otvoriti pod "odobravanjem"
                return Json(new { succses = false, message = "Nabavka je nepostojeca!" });
            if (provjerivac.RadnikSifra == narucilac.Nivo1OdobravanjaSifra)//provjera ako je tim lider 
            {
                // ako nema nadredjenog viseg nivoa a odobreno je, salji u skladiste ili marketing
                if (narucilac.Nivo2OdobravanjaSifra == null && narucilac.Nivo3OdobravanjaSifra == null && odobreno)
                {
                    nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa1 = DateTime.Now;
                    nabavkaDB.StatusNabavkeId = (int)Enum.Status.Odobreno;//status odobreno

                    poslano = PosaljiMejlMarketinguZaNovuNabavku(nabavkaDB.SerijskiBroj);
                    nabavkaDB.StatusNabavkeId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                                                      //salje mejl  i naruciocu da je narudzba odobrena i da je otisla u marketing radi narudžbe artikala
                    if (poslano)
                    {
                        poslano = false;
                        naslov_Mejla = "Odgovor na zahtjev za nabavku";
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaše <a href=" + adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" +
                             nabavka + ">nabavka </a> je odobrena, zahtjev je proslijeđen Timu prodaje i nabavki.<br/>" +
                             "Naknadno ćete biti obavješteni kada artikl/i budu spremni za preuzimanje.</p><br/><br/> ";
                        sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        na_Mejl = narucilac.Email;
                        poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
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
                            naslov_Mejla = "Zahtjev za odobravanje nabavke";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                                adresaHosta + "OdobravanjeNabavke/OdobravanjeNabavke?nabavka=" + nabavkaDB.NabavkaId + ">odobravanje nabavke</a>. <br/>" +
                                "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                                "Broj zahtjeva: " + nabavkaDB.SerijskiBroj;

                            sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = nadredjeni.Email;
                            nabavkaDB.StatusNabavkeId = 2; // status U procesu odobravanja
                            nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa1 = DateTime.Now;
                        }
                        else if (narucilac.Nivo3OdobravanjaSifra != null)
                        {
                            nadredjeni = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo3OdobravanjaSifra);
                            naslov_Mejla = "Zahtjev za odobravanje nabavke";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                               adresaHosta + "OdobravanjeNabavke/OdobravanjeNabavke?nabavka=" + nabavkaDB.NabavkaId + " >odobravanje nabavke </a>. <br/>" +
                               "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                               "Broj zahtjeva: " + nabavkaDB.SerijskiBroj;

                            sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = nadredjeni.Email;
                            nabavkaDB.StatusNabavkeId = (int)Enum.Status.U_procesu_odobravanja;  // status U procesu odobravanja
                            nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa1 = DateTime.Now;
                        }// else je nepotreban jer je slucaj kad nema ni nivoa 2 ni 3 obradjen u if-u iznad

                    }
                    else// nabavka odbijena od strane tim lidela
                    {
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaša <a href=" + adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" +
                                  nabavka + ">nabavka </a> je odbijena</p><br/> <br/>";
                        sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        naslov_Mejla = "Odgovor na zahtjev za nabavku";
                        na_Mejl = narucilac.Email;
                        nabavkaDB.StatusNabavkeId = (int)Enum.Status.Odbijeno;
                        nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa1 = DateTime.Now;
                    }
                    poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

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
                    nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa2 = DateTime.Now;


                    if (narucilac.Nivo3OdobravanjaSifra != null)// ako postoji nivo 3 odobravanja salji njemu mejl i redirektuj
                    {
                        nabavkaDB.StatusNabavkeId = (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa;//status u procesu odobravanja viseg nivoa
                        var nadredjeni = trebovanjeNabavkeContext.vRadniks.ToList().FirstOrDefault(ko => ko.RadnikSifra == narucilac.Nivo3OdobravanjaSifra);
                        naslov_Mejla = "Zahtjev za odobravanje nabavke";
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/>  Na sistemu LANACO trebovanje i nabavke je novi zahtjev za <a href=" +
                               adresaHosta + "OdobravanjeNabavke/OdobravanjeNabavke?nabavka=" + nabavkaDB.NabavkaId + ">odobravanje nabavke</a>. <br/>" +
                               "Podnosilac zahtjeva: " + narucilac.Ime + " " + narucilac.Prezime + "<br/>" +
                               "Broj zahtjeva: " + nabavkaDB.SerijskiBroj;

                        sadrzaj_Mejla += "<br/><br/><span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        na_Mejl = nadredjeni.Email;
                        poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                        if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                        {
                            trebovanjeNabavkeContext.SaveChanges();
                            return Json(new { succses = true });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspjesno slanje mejla" });
                    }
                    else if (narucilac.Nivo3OdobravanjaSifra == null)
                    {

                        poslano = PosaljiMejlMarketinguZaNovuNabavku(nabavkaDB.SerijskiBroj);
                        nabavkaDB.StatusNabavkeId = (int)Enum.Status.U_procesu_nabavke; // status "u procesu nabavke"
                        if (poslano)
                        {
                            //salje mejl  i naruciocu da je narudzba odobrena
                            poslano = false;
                            naslov_Mejla = "Odgovor na zahtjev za nabavku";
                            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaša <a href=" + adresaHosta + "Nabavka/DetaljiNabavke?nabavka=" +
                                 nabavka + ">nabavka </a> je odobrena, zahtjev je proslijeđena Timu prodaje i nabavke.<br/>" +
                                "Naknadno ćete biti obavješteni kada nabavka budu spremna za preuzimanje.</p><br/><br/> ";
                            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                            na_Mejl = narucilac.Email;
                            poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

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
                else // nabavka odbijena od strane sefa ili gen. menadzera
                {
                    sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaša <a href=" + adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" +
                                   nabavka + ">nabavka </a> je odbijena</p><br/> <br/>";
                    sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                    naslov_Mejla = "Odgovor na zahtjev za nabavku";
                    na_Mejl = narucilac.Email;
                    nabavkaDB.StatusNabavkeId = (int)Enum.Status.Odbijeno;
                    nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa2 = DateTime.Now;
                }
                poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

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
                    nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa3 = DateTime.Now;
                    nabavkaDB.StatusNabavkeId = (int)Enum.Status.U_procesu_nabavke;

                    poslano = PosaljiMejlMarketinguZaNovuNabavku(nabavkaDB.SerijskiBroj);

                    if (poslano)
                    {// salje mejl naruciocu da je nabavka odobrena 
                        poslano = false;
                        naslov_Mejla = "Odgovor na zahtjev za nabavku";
                        sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaša <a href=" + adresaHosta + "Nabavka/DetaljiNabavke?nabavka=" +
                                nabavka + ">nabavka </a> je odobrena, zahtjev je proslijeđena Timu prodaje i nabavke.<br/>" +
                               "Naknadno ćete biti obavješteni kada nabavka budu spremna za preuzimanje.</p><br/><br/> ";
                        sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                        na_Mejl = narucilac.Email;
                        poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
                        if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
                        {
                            trebovanjeNabavkeContext.SaveChanges();
                            return Json(new { succses = true });
                        }
                        else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                            return Json(new { succses = false, message = "Neuspješno slanje mejla" });
                    }
                    else Json(new { succses = false, message = "Neuspješno slanje mejla" });
                }
            }
            else // nabavka odbijena od strane sefa ili gazde
            {
                sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Vaša <a href=" + adresaHosta + "Nabavke/DetaljiNabavke?nabavka=" +
                              nabavka + ">nabavka </a> je odbijena</p><br/> <br/>";
                sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
                naslov_Mejla = "Odgovor na zahtjev za nabavku";
                na_Mejl = narucilac.Email;
                nabavkaDB.StatusNabavkeId = (int)Enum.Status.Odbijeno;

                //zamjeniti sa datumom razmatranja 
                nabavkaDB.DatumiOdobravanjaNabavke.DatumOdobravanjaNivoa3 = DateTime.Now;
            }
            poslano = email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);

            if (poslano) //AKO JE USPJESNO POSLALO REDIREKTUJ NA POCETNU STRANICU
            {
                trebovanjeNabavkeContext.SaveChanges();
                return Json(new { succses = true });
            }
            else // AKO JE DOSLO DO GRESKE I NIJE POSLALO VRATI VIEW
                return Json(new { succses = false, message = "Neuspješno slanje mejla" });
        }

        //slanje mejla u marketing kada je napravljena nova nabavka 

        //izmjeniti view koji ce otvarati - mogucnost da naruci
        public bool PosaljiMejlMarketinguZaNovuNabavku(string serijskiBroj)
        {
            var nabavka = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.SerijskiBroj == serijskiBroj);
            var marketing = nabavka.vRadnik1;//Referent nabavke 
            poslano = false;
            naslov_Mejla = "Zahtjev za novu nabavku";
            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Na sistemu LANACO trebovanje i nabavke je novi zahtjev <a href=" + adresaHosta + "Nabavke/NarucivanjeNabavke?nabavka=" +
                     nabavka.NabavkaId + "> za nabavku </a> </p> <br/><br/>";
            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
            na_Mejl = marketing.Email;
            return email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
        }

        //kada marekting naruci mejl ide u skladiste
        public bool PosaljiMejlSkladistaruZaNovuNabavku(string sifraNarucioca, int nabavka)
        {
            var narucilac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.RadnikSifra == sifraNarucioca);
            var skladistar = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(r => r.SektorSifra == "S" && r.RadnikUlogaId == 7);
            naslov_Mejla = "Zahtjev za novu nabavku";//SKLADISTARU SE SALJE LINK DO DETALJA NARUDBE KOJU TREBA NAPRAVIT (NEMAMO JOS TAJ VIEW!!!)
            sadrzaj_Mejla = "<p>Poštovani, <br/><br/> Na sistemu LANACO trebovanje i nabavke je nova <a href=" + adresaHosta + "Skladiste/PrikazNabavke?nabavka=" +
                                nabavka + ">nabavka </a> koja je odobrena radniku " + narucilac.Ime + " " + narucilac.Prezime + " iz sektora " + narucilac.NazivSektora + ".</p> <br/><br/>";
            sadrzaj_Mejla += "<span>Srdačan pozdrav,<br/><br/> Lanaco trebovanje i nabavke</span>";
            na_Mejl = skladistar.Email;
            return email.PosaljiMejl(naslov_Mejla, na_Mejl, sadrzaj_Mejla);
        }


        //[Authorize(Roles = "Tim lider,Menadzer,Generalni menadzer")]// provjeriti da li treba autorizacija, zbog slucaja kad radnik ima pravo odobravnaja nekom
        public ActionResult OdobravanjeListeNabavki()
        {
            List<NabavkaViewModel> listaTrebovanja = new List<NabavkaViewModel>();
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            if (provjerivac.RadnikUlogaId == 8)
            {
                listaTrebovanja = VratiListuNabavkiZaPregledOdobravanja(1, 1);
            }
            else if (provjerivac.RadnikUlogaId == 2)
            {
                listaTrebovanja = VratiListuNabavkiZaPregledOdobravanja(1, 1);
            }
            else if (provjerivac.RadnikUlogaId == 3)
            {
                listaTrebovanja = VratiListuNabavkiZaPregledOdobravanja(2, 2);
            }
            else if (provjerivac.RadnikUlogaId == 4)
            {
                listaTrebovanja = VratiListuNabavkiZaPregledOdobravanja(4, 3);
            }
            return View(listaTrebovanja);
        }


        public List<NabavkaViewModel> VratiListuNabavkiZaPregledOdobravanja(int statusNabavkeId, int nivoOdobravanja)
        {
            var provjerivac = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == User.Identity.Name);
            List<Nabavke> listaDbNabavki;
            List<NabavkaViewModel> listaNabavki = new List<NabavkaViewModel>();

            // provjerava da je logovai radnik koji provjerava (odobrava) prvi nivo za odobravanja radnika koji je izvrsio nabavku i da je status nabavke na cekanju (prvi status nakon izvrsavanja nabavke)
            if (nivoOdobravanja == 1)
            {
                listaDbNabavki = trebovanjeNabavkeContext.Nabavkes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo1OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusNabavkeId == statusNabavkeId)).ToList();

                listaNabavki = PopunjavanjeAtributa(listaDbNabavki);
            }
            else if (nivoOdobravanja == 2)
            {
                listaDbNabavki = trebovanjeNabavkeContext.Nabavkes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo2OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusNabavkeId == statusNabavkeId)).ToList();

                listaNabavki = PopunjavanjeAtributa(listaDbNabavki);
            }
            else if (nivoOdobravanja == 3)
            {
                listaDbNabavki = trebovanjeNabavkeContext.Nabavkes.Where(
                               t => trebovanjeNabavkeContext.vRadniks.Any(r =>
                               r.Nivo3OdobravanjaSifra == provjerivac.RadnikSifra && t.SifraRadnika == r.RadnikSifra && t.StatusNabavkeId == statusNabavkeId)).ToList();

                listaNabavki = PopunjavanjeAtributa(listaDbNabavki);
            }

            listaNabavki.OrderByDescending(t => t.DatumPodnosenjaZahtjeva);
            return listaNabavki;
        }

        //metoda koja vraca nabavkuViewModel kada joj se proslijedi listaNabavkiDbNModel koja je filtrirana zavisno od mjesta poziva ove metode
        public List<NabavkaViewModel> PopunjavanjeAtributa(List<Nabavke> listaDbNabavki)
        {

            List<NabavkaViewModel> listaNabavki = listaDbNabavki.Select(n => new NabavkaViewModel
            {
                NabavkaId = n.NabavkaId,
                SerijskiBroj = n.SerijskiBroj,
                NapomenaReferentaNabavke = n.NapomenaReferentaNabavke,
                NapomenaSefa = n.NapomenaSefa,
                DatumPodnosenjaZahtjeva = n.DatumPodnosenjaZahtjeva,
                DatumPreuzimanja = n.DatumPreuzimanja,
                SifraRadnika = n.SifraRadnika,
                Obrazlozenja = n.Obrazlozenja,
                Odgovoran = n.Odgovoran,
                SifraReferentaNabavke = n.SifraReferentaNabavke,
                StatusNabavkeId = n.StatusNabavkeId,
                NazivStatusa = n.Status.StatusNaziv,
                TipId = n.TipId,
                VezanaNabavkaId = n.VezanaNabavkaId,
                ImeIPrezimeRadnika = n.vRadnik.Ime + " " + n.vRadnik.Prezime,
                //Kupljenje dokumenata iz baze za traženu nabavku
                Dokumenti = n.Dokuments.Select(d => new DokumentViewModel(d.Dokument1, d.Naziv, d.DokumentId)).ToList(),
                //Kupljenje stavki iz baze za traženu nabavku
                Stavke = n.NabavkaVeznas.Select(s => new NabavkaVeznaViewModel
                {
                    NabavkaVeznaId = s.NabavkaVeznaId,
                    Kolicina = s.Kolicina,
                    Cijena = s.Cijena,
                    Dobavljac = s.Dobavljac,
                    Opis = s.Opis,
                    Spremno = s.StatusId == (int)Enum.Status.Spremno_za_preuzimanje || s.StatusId == (int)Enum.Status.Preuzeto ? true : false,
                    Preuzeto = s.StatusId == (int)Enum.Status.Preuzeto ? true : false
                }).ToList()
            }).OrderByDescending(t => t.DatumPodnosenjaZahtjeva).ToList();

            return listaNabavki;
        }

        //poziva se nakon kriranja nove nabavke
        public bool SlanjeMejlaZaNovuNabavku(string naslov, string sadrzaj, string korisnickoImeLogovanog, string serijskiBrojNabavke)
        {
            bool poslano = false;
            var emailPoruka = new MailMessage
            {
                Subject = naslov,
                IsBodyHtml = true,
                Body = sadrzaj
            };

            // logovani korisnik = korisnik koji vrsi nabavku 
            var logovaniKorisnik = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(k => k.KorisnickoIme == korisnickoImeLogovanog);
            var nabavka = trebovanjeNabavkeContext.Nabavkes.FirstOrDefault(t => t.SerijskiBroj == serijskiBrojNabavke);
            var mejlNadredjeni = "";

            if (logovaniKorisnik.Nivo1OdobravanjaSifra != null)
            {
                nabavka.StatusNabavkeId = (int)Enum.Status.Na_čekanju;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo1OdobravanjaSifra).Email;
            }
            else if (logovaniKorisnik.Nivo2OdobravanjaSifra != null)
            {
                nabavka.StatusNabavkeId = (int)Enum.Status.U_procesu_odobravanja;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo2OdobravanjaSifra).Email;
            }
            else if (logovaniKorisnik.Nivo3OdobravanjaSifra != null)
            {
                nabavka.StatusNabavkeId = (int)Enum.Status.U_procesu_odobravanja_viseg_nivoa;
                mejlNadredjeni = trebovanjeNabavkeContext.vRadniks.FirstOrDefault(n => n.RadnikSifra == logovaniKorisnik.Nivo3OdobravanjaSifra).Email;
            }

            if (mejlNadredjeni != "")// ukoliko pronadje nadredjenog u bilo kom nivou mejl ide njemu
            {
                emailPoruka.To.Add(mejlNadredjeni);
                try
                {
                    using (var smtp = new SmtpClient())
                    {
                        smtp.Send(emailPoruka);
                        poslano = true;
                        trebovanjeNabavkeContext.SaveChanges();
                    }
                }
                catch 
                {
                    poslano = false;
                }
            }
            else//ukoliko radnik nema nadredjenog mejl ide direktno Timu prodaja i nabavke, da izvrse nabavku
            {
                poslano = PosaljiMejlMarketinguZaNovuNabavku(serijskiBrojNabavke);
                trebovanjeNabavkeContext.SaveChanges();
            }


            return poslano;
        }

    }
}