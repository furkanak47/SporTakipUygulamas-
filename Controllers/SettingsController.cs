using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporTakip.Data;
using SporTakip.Models;
using SporTakip.Services;

namespace SporTakip.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICalorieService _calorieService;

    public SettingsController(ApplicationDbContext context, ICalorieService calorieService)
    {
        _context = context;
        _calorieService = calorieService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        
        // BMI Hesapla ve Göster
        if (user != null && user.Height > 0 && user.Weight > 0)
        {
            double boyMetre = user.Height / 100.0;
            double bmi = user.Weight / (boyMetre * boyMetre);
            ViewBag.BMI = Math.Round(bmi, 2);
            
            if (bmi < 18.5) ViewBag.Durum = "Zayıf";
            else if (bmi < 25) ViewBag.Durum = "Normal";
            else if (bmi < 30) ViewBag.Durum = "Fazla Kilolu";
            else ViewBag.Durum = "Obezite";
        }

        // Güncel kalori ve makro hedeflerini hesapla
        if (user != null)
        {
            var plan = _calorieService.GetPlanForUser(user);
            if (plan != null)
            {
                ViewBag.HedefKalori = plan.TargetCalories;
                ViewBag.HedefProtein = plan.TargetProteinGrams;
                ViewBag.HedefYag = plan.TargetFatGrams;
                ViewBag.HedefKarb = plan.TargetCarbGrams;
            }
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Update(ApplicationUser model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            // Kullanıcıdan gelen yeni bilgileri kaydet
            user.Age = model.Age;
            user.Height = model.Height;
            user.Weight = model.Weight;
            user.Gender = model.Gender;
            user.ActivityLevel = model.ActivityLevel;
            user.GoalType = model.GoalType;

            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction("Index");
    }
}