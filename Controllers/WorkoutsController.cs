using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;

namespace SporTakip.Controllers;

[Authorize]
public class WorkoutsController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkoutsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. ANTRENMAN GÜNLÜĞÜM
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null)
        {
            return Challenge();
        }
        
        var logs = await _context.WorkoutLogs
            .Include(w => w.Exercise)
            .Where(w => w.UserId == userId && w.Date.Date == DateTime.Today)
            .ToListAsync();

        return View(logs);
    }

    // 2. ANTRENMAN EKLEME SAYFASI
   // WorkoutsController.cs -> Create (GET)
    public IActionResult Create()
    {
        // Egzersizleri isme göre sıralayıp gönderiyoruz
        // Ayrıca Category bilgisini de gönderiyoruz ki Javascript okuyabilsin
        ViewBag.ExerciseList = _context.Exercises.OrderBy(x => x.Name).ToList();
        return View();
    }

    // 3. KAYDETME İŞLEMİ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkoutLog log)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ExerciseList = _context.Exercises.OrderBy(x => x.Name).ToList();
            return View(log);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge();
        }

        log.UserId = userId;
        log.Date = DateTime.Now;

        // Yakılan Kaloriyi Hesapla: (Dakika / 60) * SaatlikKalori
        var exercise = await _context.Exercises.FindAsync(log.ExerciseId);
        if (exercise != null)
        {
            log.CaloriesBurned = (log.DurationMinutes / 60.0) * exercise.CaloriesPerHour;
        }

        _context.Add(log);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // 4. SİLME
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge();
        }

        var log = await _context.WorkoutLogs.FindAsync(id);
        if (log != null && log.UserId == userId)
        {
            _context.WorkoutLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}