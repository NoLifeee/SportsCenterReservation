using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsCenterReservation.Data;
using SportsCenterReservation.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace SportsCenterReservation.Controllers
{
    [Authorize]
    public class RezervareController : Controller
    {
        private readonly AppDbContext _context;

        public RezervareController(AppDbContext context)
        {
            _context = context;
        }

        // Afiseaza lista de rezervari, cu optiune de cautare dupa nume client sau data
        public async Task<IActionResult> Index(string searchString)
        {
            var rezervari = _context.Rezervari
                .Include(r => r.Serviciu)
                .OrderBy(r => r.Ordine);

            // Filtreaza rezervarile daca se introduce un termen de cautare
            if (!string.IsNullOrEmpty(searchString))
            {
                rezervari = (IOrderedQueryable<Rezervare>)rezervari.Where(r =>
                    r.Client.Contains(searchString) ||
                    r.Data.ToString().Contains(searchString)
                );
            }

            return View(await rezervari.ToListAsync());
        }

        // Returneaza datele pentru calendar (formate specifice FullCalendar)
        [AllowAnonymous]
        public IActionResult GetCalendarData()
        {
            var rezervari = _context.Rezervari
                .Include(r => r.Serviciu)
                .ToList();

            var events = new List<object>();

            // Grupeaza rezervarile pe serviciu si data
            var grouped = rezervari
                .GroupBy(r => new { r.ServiciuId, r.Data.Date })
                .ToList();

            foreach (var group in grouped)
            {
                var sorted = group.OrderBy(r => TimeSpan.Parse(r.Ora)).ToList();
                TimeSpan? currentStart = null;
                TimeSpan? currentEnd = null;
                string serviceName = sorted.First().Serviciu.Nume;

                // Combina intervalele suprapuse pentru acelasi serviciu si data
                foreach (var rez in sorted)
                {
                    TimeSpan start = TimeSpan.Parse(rez.Ora);
                    TimeSpan end = start.Add(TimeSpan.FromHours(rez.DurataOre));

                    if (!currentStart.HasValue)
                    {
                        currentStart = start;
                        currentEnd = end;
                    }
                    else if (start <= currentEnd)
                    {
                        currentEnd = end;
                    }
                    else
                    {
                        AddEvent(group.Key.Date, currentStart.Value, currentEnd.Value, serviceName, events);
                        currentStart = start;
                        currentEnd = end;
                    }
                }

                // Adauga ultimul interval procesat
                if (currentStart.HasValue)
                {
                    AddEvent(group.Key.Date, currentStart.Value, currentEnd.Value, serviceName, events);
                }
            }

            return Json(events);
        }

        // Helper pentru a formata evenimentele calendarului
        private void AddEvent(DateTime date, TimeSpan start, TimeSpan end, string serviceName, List<object> events)
        {
            DateTime startLocal = date.Add(start);
            DateTime endLocal = date.Add(end);

            DateTime startUtc = startLocal.ToUniversalTime();
            DateTime endUtc = endLocal.ToUniversalTime();

            string titleWithTime = $"{serviceName} {startLocal:HH:mm}";

            events.Add(new
            {
                title = titleWithTime,
                start = startUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'"),
                end = endUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'")
            });
        }

        // Formular pentru crearea unei noi rezervari
        public IActionResult Create()
        {
            // Preia serviciile sortate dupa numarul de rezervari (descrescator)
            var serviciiCuRezervari = _context.Servicii
                .Select(s => new
                {
                    s.Id,
                    s.Nume,
                    NumarRezervari = _context.Rezervari.Count(r => r.ServiciuId == s.Id)
                })
                .OrderByDescending(s => s.NumarRezervari)
                .ToList();

            // Formateaza textul pentru dropdown (ex: "Tenis (5 rezervari)")
            ViewBag.Servicii = new SelectList(
                serviciiCuRezervari.Select(s => new
                {
                    s.Id,
                    DisplayText = $"{s.Nume} ({s.NumarRezervari} {(s.NumarRezervari == 1 ? "rezervare)" : "rezervări)")}"
                }),
                "Id",
                "DisplayText"
            );
            return View();
        }

        // Proceseaza formularul de creare rezervare
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rezervare rezervare)
        {
            rezervare.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Validare: Data nu poate fi in trecut
            if (rezervare.Data < DateTime.Today)
            {
                ModelState.AddModelError("Data", "Data trebuie să fie cel puțin ziua de mâine.");
            }

            // Validare: Format ore
            TimeSpan oraStart;
            bool oraValida = TimeSpan.TryParse(rezervare.Ora, out oraStart);
            if (!oraValida)
            {
                ModelState.AddModelError("Ora", "Ora nu este într-un format valid.");
            }
            else
            {
                TimeSpan durata = TimeSpan.FromHours(rezervare.DurataOre);
                TimeSpan oraFinal = oraStart.Add(durata);

                // Verifica suprapuneri cu rezervarile existente
                var rezervariExistente = _context.Rezervari
                    .Where(r => r.Data == rezervare.Data && r.ServiciuId == rezervare.ServiciuId)
                    .ToList();

                foreach (var r in rezervariExistente)
                {
                    if (TimeSpan.TryParse(r.Ora, out TimeSpan oraExistenta))
                    {
                        TimeSpan durataExistenta = TimeSpan.FromHours(r.DurataOre);
                        TimeSpan oraSfarsitExistenta = oraExistenta.Add(durataExistenta);

                        // Detecteaza suprapuneri de intervale
                        bool seSuprapun = oraStart < oraSfarsitExistenta && oraFinal > oraExistenta;
                        if (seSuprapun)
                        {
                            ModelState.AddModelError("Ora", $"Intervalul {oraStart:hh\\:mm} - {oraFinal:hh\\:mm} se suprapune cu o rezervare existentă ({oraExistenta:hh\\:mm} - {oraSfarsitExistenta:hh\\:mm}). Alege alt interval!");
                            break;
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(rezervare);
                TempData["SuccessMessage"] = "Rezervare creată cu succes!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reincarca dropdown-ul cu servicii daca validarea esueaza
            ViewBag.Servicii = new SelectList(_context.Servicii, "Id", "Nume");
            return View(rezervare);
        }

        // Formular pentru editarea unei rezervari existente
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rezervari == null)
            {
                return NotFound();
            }

            var rezervare = await _context.Rezervari.FindAsync(id);
            if (rezervare == null)
            {
                return NotFound();
            }

            ViewBag.Servicii = new SelectList(_context.Servicii, "Id", "Nume", rezervare.ServiciuId);
            return View(rezervare);
        }

        // Proceseaza formularul de editare rezervare
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rezervare rezervare)
        {
            if (id != rezervare.Id) return NotFound();

            // Validare: Data nu poate fi in trecut
            if (rezervare.Data < DateTime.Today)
            {
                ModelState.AddModelError("Data", "Data trebuie să fie cel puțin ziua de mâine.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            rezervare.UserId = currentUserId;

            // Validare: Format ore si suprapuneri (excluzand rezervarea curenta)
            TimeSpan oraStart;
            bool oraValida = TimeSpan.TryParse(rezervare.Ora, out oraStart);
            if (!oraValida)
            {
                ModelState.AddModelError("Ora", "Ora nu este într-un format valid.");
            }
            else
            {
                TimeSpan durata = TimeSpan.FromHours(rezervare.DurataOre);
                TimeSpan oraFinal = oraStart.Add(durata);

                var rezervariExistente = _context.Rezervari
                    .Where(r => r.Data == rezervare.Data && r.ServiciuId == rezervare.ServiciuId && r.Id != rezervare.Id)
                    .ToList();

                foreach (var r in rezervariExistente)
                {
                    if (TimeSpan.TryParse(r.Ora, out TimeSpan oraExistenta))
                    {
                        TimeSpan durataExistenta = TimeSpan.FromHours(r.DurataOre);
                        TimeSpan oraSfarsitExistenta = oraExistenta.Add(durataExistenta);

                        // Detecteaza suprapuneri de intervale
                        bool seSuprapun = oraStart < oraSfarsitExistenta && oraFinal > oraExistenta;
                        if (seSuprapun)
                        {
                            ModelState.AddModelError("Ora", $"Suprapunere cu rezervarea existentă: {oraExistenta:hh\\:mm} - {oraSfarsitExistenta:hh\\:mm}");
                            break;
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezervare);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rezervare editată cu succes!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervareExists(rezervare.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Reincarca dropdown-ul cu servicii daca validarea esueaza
            ViewBag.Servicii = new SelectList(_context.Servicii, "Id", "Nume", rezervare.ServiciuId);
            return View(rezervare);
        }

        // Verifica existenta unei rezervari dupa ID
        private bool RezervareExists(int id)
        {
            return _context.Rezervari.Any(e => e.Id == id);
        }

        // Confirmare stergere rezervare
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var rezervare = await _context.Rezervari.FirstOrDefaultAsync(m => m.Id == id);
            if (rezervare == null) return NotFound();

            return View(rezervare);
        }

        // Proceseaza stergerea rezervarii
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervare = await _context.Rezervari.FindAsync(id);
            if (rezervare != null)
            {
                _context.Rezervari.Remove(rezervare);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervare ștearsă cu succes!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Actualizeaza ordinea rezervarilor (accesibil doar pentru Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizeazaOrdine([FromBody] List<RezervareOrdineDto> ordine)
        {
            try
            {
                foreach (var item in ordine)
                {
                    var rezervare = await _context.Rezervari.FindAsync(item.Id);
                    if (rezervare != null)
                    {
                        rezervare.Ordine = item.Ordine; // Actualizeaza proprietatea Ordine
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Eroare la salvarea ordinii");
            }
        }

        // Model DTO pentru actualizarea ordinii rezervarilor
        public class RezervareOrdineDto
        {
            public int Id { get; set; }
            public int Ordine { get; set; }
        }
    }
}