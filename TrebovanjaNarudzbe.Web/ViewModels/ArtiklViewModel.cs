using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class ArtiklViewModel
    {
        public int ArtiklId { get; set; }
        public decimal Cijena { get; set; }
        public string Naziv { get; set; }

        [Range(1, Int32.MaxValue)]
        public int KolicinaNaStanju { get; set; }

        //Provjera kolicine na stanju
        public int RezervisanaKolicina { get; set; }
        public int TrebovanaKolicina { get; set; }
        public int NedostajucaKolicna { get; set; }
        
        public bool Spremno { get; set; }
        public bool Preuzeto { get; set; }
    }
}