using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class TrebovanjeVeznaViewModel
    {
        public int TrebovanjeVeznaId { get; set; }

        public int? ArtikalId { get; set; }
        
        public int TrebovanjeId { get; set; }
         
        [Display(Name = "Status trebovanja")]
        public int StatusTrebovanjaId { get; set; }

        [Range(1, 100)]
        [Display(Name = "Količina")]
        [Required(ErrorMessage = "Polje \"{0}\" je obavezno.")]
        public int Kolicina { get; set; }
    }
}