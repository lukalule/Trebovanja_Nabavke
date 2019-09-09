using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class RazlikaViewModel
    {
        public int? ArtikalId { get; set; }
        public int TrebovanaKolicina { get; set; }
        public int NedostajucaKolicna { get; set; }
    }
}