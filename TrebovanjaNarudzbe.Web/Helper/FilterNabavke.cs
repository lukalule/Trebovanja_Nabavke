using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using TrebovanjaNarudzbe.Web.ViewModels;

namespace TrebovanjaNarudzbe.Web.Helper
{
    public class FilterNabavke
    {
        private string SortiranjePoImenu { get; set; }
        private string SortiranjeDatum { get; set; }
        private string StatusTrebovanja { get; set; }
        public List<NabavkaViewModel> Lista { get; set; }

        public FilterNabavke(string SortiranjePoImenu, string SortiranjeDatum, string StatusTrebovanja, List<NabavkaViewModel> lista)
        {
            this.SortiranjePoImenu = SortiranjePoImenu;
            this.StatusTrebovanja = StatusTrebovanja;
            this.SortiranjeDatum = SortiranjeDatum;
            this.Lista = FilterZaNabavke(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, lista);
        }
        public FilterNabavke(string SortiranjePoImenu, string SortiranjeDatum, List<NabavkaViewModel> lista)
        {
            this.SortiranjePoImenu = SortiranjePoImenu;
            this.SortiranjeDatum = SortiranjeDatum;
            StatusTrebovanja = "";
            this.Lista = FilterZaNabavke(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, lista);

        }

        private List<NabavkaViewModel> FilterZaNabavke(string SortiranjePoImenu, string SortiranjeDatum, string StatusTrebovanja, List<NabavkaViewModel> lista)
        {


            //sortiranje po nazivu artikla i imenu korisnika
            if (!string.IsNullOrEmpty(SortiranjePoImenu) && !string.IsNullOrWhiteSpace(SortiranjePoImenu))
            {
                lista = FilterArtikliKorisnici(SortiranjePoImenu, lista);
            }

            //najstarijeg ka najnovijem  == 1
            //najnovijeg ka najstarijem == 2
            //validacija-provjera vrijednosti inputa za datum
            if (SortiranjeDatum == "1" || SortiranjeDatum == "2")
            {
                lista = FilterPoDatumu(lista, SortiranjeDatum);
            }

            if (!string.IsNullOrEmpty(StatusTrebovanja) && StatusTrebovanja != "Prikazi sve...")
            {
                lista = FilterPoStatusu(lista, StatusTrebovanja);
            }

            return lista;


        }


        private List<NabavkaViewModel> FilterArtikliKorisnici(string txtPretraga, List<NabavkaViewModel> aktivnaTrebovanja)
        {
            txtPretraga = txtPretraga.Trim();
            string[] rijeci = txtPretraga.Split(' ');

            //var aktivnaTrebovanja = GetAktivnaTrebovanja(); //Sva aktivna trebovanja, treba ih filtrirati..
            List<NabavkaViewModel> filtriranaTrebovanja = new List<NabavkaViewModel>();
            if (rijeci.Length == 2) //pretpostavka da je ukucano ime i prezime
            {
                filtriranaTrebovanja = aktivnaTrebovanja.Where(t => FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(FilterTrebovanja.VratiNormalizovanString(rijeci[0])) &&
                                                                    FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(FilterTrebovanja.VratiNormalizovanString(rijeci[1]))).ToList();
            }
            if (filtriranaTrebovanja.Count == 0)
            {
                filtriranaTrebovanja = aktivnaTrebovanja.Where(t =>FilterTrebovanja.VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(FilterTrebovanja.VratiNormalizovanString(txtPretraga)) ||
                t.Stavke.Any(a => FilterTrebovanja.VratiNormalizovanString(a.Opis).Contains(FilterTrebovanja.VratiNormalizovanString(txtPretraga)))).ToList();///
            }
            return filtriranaTrebovanja;

        }
        private List<NabavkaViewModel> FilterPoDatumu(List<NabavkaViewModel> listaTrebovanja, string SortiranjeDatum)
        {
            //filter od najstarijeg ka najnovijem  == 1
            //filter od najnovijeg ka najstarijem ==  2 
            switch (SortiranjeDatum)
            {
                case "1":
                    listaTrebovanja = listaTrebovanja.OrderBy(o => o.DatumPodnosenjaZahtjeva).ToList();
                    break;
                case "2":
                    listaTrebovanja = listaTrebovanja.OrderByDescending(o => o.DatumPodnosenjaZahtjeva).ToList();
                    break;
            }
            return listaTrebovanja;
        }
        private List<NabavkaViewModel> FilterPoStatusu(List<NabavkaViewModel> listaTrebovanja, string statusTrebovanja)
        {
            //int statusId;
            try
            {

                // statusId = TrebovanjeNabavkeContext.Status.FirstOrDefault(o => o.StatusNaziv == statusTrebovanja).StatusId;
                List<NabavkaViewModel> lista = listaTrebovanja.Where(x => x.NazivStatusa == statusTrebovanja).ToList();
                return lista;
            }
            catch (Exception)
            {
                //ako ne postoji status id u bazi vrati listu 
                return listaTrebovanja;
            }

        }

      
    }
}