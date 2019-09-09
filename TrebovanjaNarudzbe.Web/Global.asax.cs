using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using TrebovanjaNarudzbe.Models.Models;

namespace TrebovanjaNarudzbe.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.IsAuthenticated)
            {
                TrebovanjeNabavkeContext trebovanjeNabavkeContext = new TrebovanjeNabavkeContext();
                var authCookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null)
                {
                    var authTicket = new FormsAuthenticationTicket(
                                                            1,
                                                            User.Identity.Name,
                                                            DateTime.Now,
                                                            DateTime.Now.AddMinutes(30),
                                                            false,
                                                            trebovanjeNabavkeContext.vRadniks.FirstOrDefault(t => t.KorisnickoIme == User.Identity.Name).NazivUloge

                                                            //string.Join(",", korisnik.KorisnikUlogas.Select(o => o.Uloga.Naziv).Distinct())// ukoliko jedan radnik moze imati vise uloga
                                                            );

                    var encTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                    Response.Cookies.Add(cookie);
                    authCookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
                }
                if (authCookie != null)
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    ctx.User = new System.Security.Principal.GenericPrincipal(
                        new System.Security.Principal.GenericIdentity(authTicket.Name, "Windows"), authTicket.UserData.Split(','));
                }
            }
        }
    }
}
