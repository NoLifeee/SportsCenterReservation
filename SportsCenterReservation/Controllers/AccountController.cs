using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsCenterReservation.Data;
using System.Security.Claims;

namespace SportsCenterReservation.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Acceseaza pagina de autentificare
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Proceseaza datele de autentificare
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Creeaza sesiunea de autentificare
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = false
                    });

                // Redirectioneaza catre URL-ul specificat (daca este valid)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // Mesaj de eroare pentru utilizator
            ViewBag.Error = "Date de autentificare incorecte.";
            return View();
        }

        // POST: Delogheaza utilizatorul
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Delogheaza si sterge cookie-ul de autentificare
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Forteaza expirarea cookie-urilor
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Login", "Account");
        }

        // GET: Afiseaza pagina "Acces interzis"
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}