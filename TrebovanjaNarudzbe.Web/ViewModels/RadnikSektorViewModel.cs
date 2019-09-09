using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class RadnikSektorViewModel
    {
        public int RadnikSektorId { get; set; }
        public string RadnikSifra { get; set; }
        public string SektorSifra { get; set; }
        public byte RadnikUlogaId { get; set; }
    }
}