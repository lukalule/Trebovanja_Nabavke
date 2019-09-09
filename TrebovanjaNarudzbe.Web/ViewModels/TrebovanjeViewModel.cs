using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class TrebovanjeViewModel
    {
        public int TrebovanjeId { get; set; }

        [Display(Name = "Šifra radnika")]
        [Required(ErrorMessage = "Polje \"{0}\" je obavezno.")]
        [StringLength(4, MinimumLength = 4)]
        public string SifraRadnika { get; set; }

        public string  ImeIPrezimeRadnika { get; set; }

        [Display(Name = "Serijski broj")]
        [StringLength(20)]
        public string SerijskiBroj { get; set; }

        public string NazivStatusa { get; set; }

        public DateTime DatumPodnesenogZahtjeva { get; set; }

        public DateTime? DatumZaduzenjaTrebovanja { get; set; }

        [Display(Name = "Napomena radnika")]
        [StringLength(500)]
        public string NapomenaRadnika { get; set; }

        [Display(Name = "Napomena nadređenog")]
        [StringLength(500)]
        public string NapomenaNadredjenog { get; set; }

        public int StatusTrebovanjaId { get; set; }
        //public string StatusTrebovanjaNaziv { get; set; }

        public List<ArtiklViewModel> ListaArtikalaTrebovanja { get; set; }
    }
}