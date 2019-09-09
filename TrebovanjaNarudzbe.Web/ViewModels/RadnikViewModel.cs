using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class RadnikViewModel
    {
        public string RadnikSifra { get; set; }
        public string Prezime { get; set; }
        public string Ime { get; set; }
        public string KorisnickoIme { get; set; }
        public string Email { get; set; }
        public int UlogaId { get; set; }
        public string NazivUloge { get; set; }
        public string Nivo1OdobravanjaSifra { get; set; }
        public string Nivo2OdobravanjaSifra { get; set; }
        public string Nivo3OdobravanjaSifra { get; set; }
    }
}