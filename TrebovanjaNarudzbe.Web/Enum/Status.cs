using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.Enum
{
    public enum Status
    {
        Na_čekanju=1,
        U_procesu_odobravanja=2,
        Odobreno=3,
        U_procesu_odobravanja_viseg_nivoa=4,
        U_procesu_nabavke=5,
        Naruceno=6,
        Artikl_u_pripremi=7,
        Spremno_za_preuzimanje=8,
        Preuzeto=9,
        Odbijeno=10
    }
}