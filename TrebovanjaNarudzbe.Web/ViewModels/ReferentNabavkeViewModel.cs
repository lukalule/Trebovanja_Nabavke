using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class ReferentNabavkeViewModel
    {
        public int ReferentId { get; set; }
        public string SifraSektora { get; set; }
        public int TipNabavkeId { get; set; }
        public string SifraRadnika { get; set; }
        public string Napomena { get; set; }
    }
}