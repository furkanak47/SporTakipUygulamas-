using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;

namespace SporTakip.Controllers;

[Authorize]
public class ProgressController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. GRAFİK VE LİSTE EKRANI
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Kullanıcının geçmiş kilo kayıtlarını tarihe göre sıralayıp getir
        var progressList = await _context.UserProgresses
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Date)
            .ToListAsync();

        return View(progressList);
    }

    // 2. YENİ KİLO EKLEME SAYFASI
    public IActionResult Create()
    {
        return View();
    }

    // 3. KİLOYU KAYDET
    [HttpPost]
    public async Task<IActionResult> Create(UserProgress progress)
    {
        progress.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        progress.Date = DateTime.Now;

        _context.Add(progress);
        
        // Kullanıcının ana tablosundaki "Güncel Kilo" bilgisini de güncelleyelim
        var user = await _context.Users.FindAsync(progress.UserId);
        if (user != null)
        {
            user.Weight = progress.Weight;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // 4. SİLME
    public async Task<IActionResult> Delete(int id)
    {
        var progress = await _context.UserProgresses.FindAsync(id);
        if (progress != null)
        {
            _context.UserProgresses.Remove(progress);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}