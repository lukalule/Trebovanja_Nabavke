using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using TrebovanjaNarudzbe.Models.Models;
using TrebovanjaNarudzbe.Web.ViewModels;
using X.PagedList;


namespace TrebovanjaNarudzbe.Web.Helper
{
    public class FilterTrebovanja
    {
        private string SortiranjePoImenu { get; set; }
        private string SortiranjeDatum { get; set; }
        private string StatusTrebovanja { get; set; }
        public List<TrebovanjeViewModel> Lista { get; set; }

        public FilterTrebovanja(string SortiranjePoImenu, string SortiranjeDatum, string StatusTrebovanja, List<TrebovanjeViewModel> lista)
        {
            this.SortiranjePoImenu = SortiranjePoImenu;
            this.StatusTrebovanja = StatusTrebovanja;
            this.SortiranjeDatum = SortiranjeDatum;
            this.Lista = FilterZaTrebovanja(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, lista);
        }
        public FilterTrebovanja(string SortiranjePoImenu, string SortiranjeDatum, List<TrebovanjeViewModel> lista)
        {
            this.SortiranjePoImenu = SortiranjePoImenu;
            this.SortiranjeDatum = SortiranjeDatum;
            StatusTrebovanja = "";
            this.Lista = FilterZaTrebovanja(SortiranjePoImenu, SortiranjeDatum, StatusTrebovanja, lista);

        }

        private List<TrebovanjeViewModel> FilterZaTrebovanja(string SortiranjePoImenu, string SortiranjeDatum, string StatusTrebovanja, List<TrebovanjeViewModel> lista)
        {


            //sortiranje po nazivu artikla i imenu korisnika
            if (!string.IsNullOrEmpty(SortiranjePoImenu))
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


        private List<TrebovanjeViewModel> FilterArtikliKorisnici(string txtPretraga, List<TrebovanjeViewModel> aktivnaTrebovanja)
        {
            txtPretraga = txtPretraga.Trim();
            string[] rijeci = txtPretraga.Split(' ');

            //var aktivnaTrebovanja = GetAktivnaTrebovanja(); //Sva aktivna trebovanja, treba ih filtrirati..
            List<TrebovanjeViewModel> filtriranaTrebovanja = new List<TrebovanjeViewModel>();
            if (rijeci.Length == 2) //pretpostavka da je ukucano ime i prezime
            {
                filtriranaTrebovanja = aktivnaTrebovanja.Where(t => VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(VratiNormalizovanString(rijeci[0])) &&
                                                                    VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(VratiNormalizovanString(rijeci[1]))).ToList();
            }
            if (filtriranaTrebovanja.Count == 0)
            {
                filtriranaTrebovanja = aktivnaTrebovanja.Where(t => VratiNormalizovanString(t.ImeIPrezimeRadnika).Contains(VratiNormalizovanString(txtPretraga)) ||
                t.ListaArtikalaTrebovanja.Any(a => VratiNormalizovanString(a.Naziv).Contains(VratiNormalizovanString(txtPretraga)))).ToList();///
            }
            return filtriranaTrebovanja;

        }
        private List<TrebovanjeViewModel> FilterPoDatumu(List<TrebovanjeViewModel> listaTrebovanja, string SortiranjeDatum)
        {
            //filter od najstarijeg ka najnovijem  == 1
            //filter od najnovijeg ka najstarijem ==  2 
            switch (SortiranjeDatum)
            {
                case "1":
                    listaTrebovanja = listaTrebovanja.OrderBy(o => o.DatumPodnesenogZahtjeva).ToList();
                    break;
                case "2":
                    listaTrebovanja = listaTrebovanja.OrderByDescending(o => o.DatumPodnesenogZahtjeva).ToList();
                    break;
            }
            return listaTrebovanja;
        }
        private List<TrebovanjeViewModel> FilterPoStatusu(List<TrebovanjeViewModel> listaTrebovanja, string statusTrebovanja)
        {
            //int statusId;
            try
            {

               // statusId = TrebovanjeNabavkeContext.Status.FirstOrDefault(o => o.StatusNaziv == statusTrebovanja).StatusId;
                List<TrebovanjeViewModel> lista = listaTrebovanja.Where(x => x.NazivStatusa == statusTrebovanja).ToList();
                return lista;
            }
            catch (Exception)
            {
                //ako ne postoji status id u bazi vrati listu 
                return listaTrebovanja;
            }

        }

        public static string VratiNormalizovanString(string str)
        {
            string normal = str.Normalize(NormalizationForm.FormD);
            var withoutDiacritics = normal.Where(
                c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            string final = new string(withoutDiacritics.ToArray());
            if (final != str)
                str = final;
            return str.ToLower();
        }


     

    }
}