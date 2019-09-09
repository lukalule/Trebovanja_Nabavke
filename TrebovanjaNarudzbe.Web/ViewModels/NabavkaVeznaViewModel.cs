using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class NabavkaVeznaViewModel
    {
        public int NabavkaVeznaId { get; set; }
        public int NabavkaId { get; set; }
        public string Opis { get; set; }
        public int Kolicina { get; set; }
        public Nullable<decimal> Cijena { get; set; }
        public string Dobavljac { get; set; }
        public int? ArtiklId { get; set; }
        public int StatusId { get; set; }

        public bool Spremno { get; set; }
        public bool Preuzeto { get; set; }
    }
}