using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class NivoOdobrenjaTrebovanjaViewModel
    {
        public DateTime? DatumPrvogNivoaOdobrenja { get; set; }
        public DateTime? DatumDrugogNivoaOdobrenja { get; set; }
        public DateTime? DatumTrecegNivoaOdobrenja { get; set; }
        public int TrebovanjaId { get; set; }
    }
}