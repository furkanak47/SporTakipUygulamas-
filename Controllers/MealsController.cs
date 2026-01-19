using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;
using SporTakip.Services;

namespace SporTakip.Controllers;

[Authorize]
public class MealsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICalorieService _calorieService;

    public MealsController(ApplicationDbContext context, ICalorieService calorieService)
    {
        _context = context;
        _calorieService = calorieService;
    }

    // 1. GÜNLÜĞÜ VE HEDEFLERİ LİSTELE
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null)
        {
            return Challenge();
        }
        
        // Kullanıcıyı ve Yemeklerini Çek
        var user = await _context.Users.FindAsync(userId);
        var todaysMeals = await _context.MealLogs
            .Include(m => m.Food)
            .Where(m => m.UserId == userId && m.Date.Date == DateTime.Today)
            .ToListAsync();

        // --- AKILLI HESAPLAMA MOTORU ---
        double gunlukKaloriHedefi = 2000; // Varsayılan
        
        if (user != null)
        {
            var plan = _calorieService.GetPlanForUser(user);
            if (plan != null)
            {
                gunlukKaloriHedefi = plan.TargetCalories;

                // Değerleri Ekrana Gönder
                ViewBag.HedefKalori = plan.TargetCalories;
                ViewBag.HedefProtein = plan.TargetProteinGrams;
                ViewBag.HedefYag = plan.TargetFatGrams;
                ViewBag.HedefKarb = plan.TargetCarbGrams;
            }
        }

        if (ViewBag.HedefKalori == null)
        {
            // Kullanıcı profili eksikse yine de bir varsayılan hedef gönder
            ViewBag.HedefKalori = (int)gunlukKaloriHedefi;
            ViewBag.HedefProtein = (int)((gunlukKaloriHedefi * 0.25) / 4); // %25 Protein
            ViewBag.HedefYag = (int)((gunlukKaloriHedefi * 0.30) / 9);     // %30 Yağ
            ViewBag.HedefKarb = (int)((gunlukKaloriHedefi * 0.45) / 4);    // %45 Karbonhidrat
        }

        return View(todaysMeals);
    }

    // 2. YEMEK EKLEME SAYFASI
    public IActionResult Create()
    {
        // Yemek listesini tüm detaylarıyla View'a gönderiyoruz
        ViewBag.FoodList = _context.Foods.OrderBy(f => f.Name).ToList();
        return View();
    }

    // 3. YEMEK KAYDET
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MealLog mealLog)
    {
        ViewBag.FoodList = _context.Foods.OrderBy(f => f.Name).ToList();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge();
        }

        // Yemeğin var olup olmadığını kontrol et
        if (mealLog.FoodId <= 0)
        {
            ModelState.AddModelError("FoodId", "Lütfen bir yemek seçin!");
            return View(mealLog);
        }

        var food = await _context.Foods.FindAsync(mealLog.FoodId);
        if (food == null)
        {
            ModelState.AddModelError("FoodId", "Seçilen yemek bulunamadı!");
            return View(mealLog);
        }

        // Güvenlik kontrolü: Çok büyük gramaj değerlerini engelle
        if (mealLog.QuantityInGrams <= 0)
        {
            ModelState.AddModelError("QuantityInGrams", "Miktar 0'dan büyük olmalıdır! Lütfen porsiyon veya gram miktarını girin.");
            return View(mealLog);
        }

        if (mealLog.QuantityInGrams > 5000)
        {
            ModelState.AddModelError("QuantityInGrams", $"Miktar 5000 gramdan (5 kg) fazla olamaz! Girdiğiniz değer: {mealLog.QuantityInGrams:F2} gram. Lütfen daha küçük bir miktar girin.");
            return View(mealLog);
        }

        if (!ModelState.IsValid)
        {
            return View(mealLog);
        }

        mealLog.UserId = userId;
        mealLog.Date = DateTime.Now;
        _context.Add(mealLog);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // 4. SİL
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge();
        }

        var meal = await _context.MealLogs.FindAsync(id);
        if (meal != null && meal.UserId == userId)
        {
            _context.MealLogs.Remove(meal);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}