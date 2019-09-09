using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class NabavkaViewModel
    {
        public int NabavkaId { get; set; }
        public string SifraRadnika { get; set; }
        public string SifraReferentaNabavke { get; set; }
        public int TipId { get; set; }
        public int StatusNabavkeId { get; set; }
        public string NazivStatusa { get; set; }
        public string SerijskiBroj { get; set; }
        public DateTime DatumPodnosenjaZahtjeva { get; set; }
        public Nullable<DateTime> DatumPreuzimanja { get; set; }
        public string Obrazlozenja { get; set; }
        public string NapomenaSefa { get; set; }
        public string NapomenaReferentaNabavke { get; set; }
        public Nullable<int> VezanaNabavkaId { get; set; }
        public string Odgovoran { get; set; }
        public string ImeIPrezimeRadnika { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }

        public string TipNabavke { get; set; }

        //[Required(ErrorMessage = "Selektujte dokument/e.")]
        [Display(Name = "Browse")]
        public List<DokumentViewModel> Dokumenti { get; set; }

        public List<NabavkaVeznaViewModel> Stavke { get; set; }

        public NabavkaViewModel()
        {
            Files = new List<HttpPostedFileBase>();
        }
    }
}