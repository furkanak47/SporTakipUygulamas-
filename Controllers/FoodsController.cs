using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;

namespace SporTakip.Controllers;

[Authorize] // Sadece giriş yapmış üyeler yemekleri görebilsin
public class FoodsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FoodsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. YEMEKLERİ LİSTELE
    public async Task<IActionResult> Index()
    {
        return View(await _context.Foods.ToListAsync());
    }

    // 2. YENİ YEMEK EKLEME SAYFASINI AÇ
    public IActionResult Create()
    {
        return View();
    }

    // 3. YENİ YEMEK KAYDET
    [HttpPost]
    public async Task<IActionResult> Create(Food food)
    {
        // GramsPerServing validasyonu
        if (food.GramsPerServing <= 0)
        {
            food.GramsPerServing = 100; // Varsayılan değer
        }
        
        // Güvenlik kontrolü: Çok büyük değerleri engelle
        if (food.GramsPerServing > 1000)
        {
            ModelState.AddModelError("GramsPerServing", "1 porsiyon 1000 gramdan fazla olamaz!");
            return View(food);
        }
        
        if (ModelState.IsValid)
        {
            _context.Add(food);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(food);
    }
    
    // 4. YEMEK SİL
    public async Task<IActionResult> Delete(int id)
    {
        var food = await _context.Foods.FindAsync(id);
        if (food != null)
        {
            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}