using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;
using SporTakip.Services;
using SporTakip.ViewModels;

namespace SporTakip.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICalorieService _calorieService;

    public HomeController(ApplicationDbContext context, ICalorieService calorieService)
    {
        _context = context;
        _calorieService = calorieService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            Today = DateTime.Today,
            UserName = User.Identity?.Name ?? ""
        };

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId != null)
            {
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    model.HasCompleteProfile = user.Height > 0 && user.Weight > 0 && user.Age > 0;

                    // 1. BMI Hesapla
                    if (user.Height > 0 && user.Weight > 0)
                    {
                        double boyMetre = user.Height / 100.0;
                        double bmi = user.Weight / (boyMetre * boyMetre);
                        model.Bmi = Math.Round(bmi, 2);

                        if (bmi < 18.5) { model.BmiStatus = "Zayıf"; model.BmiBadgeColor = "warning"; }
                        else if (bmi < 25) { model.BmiStatus = "Normal"; model.BmiBadgeColor = "success"; }
                        else if (bmi < 30) { model.BmiStatus = "Fazla Kilolu"; model.BmiBadgeColor = "warning"; }
                        else { model.BmiStatus = "Obezite"; model.BmiBadgeColor = "danger"; }
                    }

                    // Kalori ve makro hedeflerini hesapla
                    var plan = _calorieService.GetPlanForUser(user);
                    if (plan != null)
                    {
                        model.TargetCalories = plan.TargetCalories;
                        model.TargetProtein = plan.TargetProteinGrams;
                        model.TargetFat = plan.TargetFatGrams;
                        model.TargetCarbs = plan.TargetCarbGrams;
                    }
                }

                // 2. YEMEKLER (Alınan Kalori ve Makro Besinler)
                var bugunYenenler = await _context.MealLogs
                    .Include(m => m.Food)
                    .Where(m => m.UserId == userId && m.Date.Date == DateTime.Today)
                    .ToListAsync();

                if (bugunYenenler != null && bugunYenenler.Count > 0)
                {
                    var alinanKalori = bugunYenenler.Sum(x => (x.Food!.CaloriesPer100g * x.QuantityInGrams) / 100);
                    model.CaloriesIn = (int)alinanKalori;
                    
                    // Makro besinler
                    model.ProteinIn = bugunYenenler.Sum(x => (x.Food!.ProteinPer100g * x.QuantityInGrams) / 100);
                    model.FatIn = bugunYenenler.Sum(x => (x.Food!.FatPer100g * x.QuantityInGrams) / 100);
                    model.CarbsIn = bugunYenenler.Sum(x => (x.Food!.CarbsPer100g * x.QuantityInGrams) / 100);
                }

                // 3. SPORLAR (Yakılan Kalori)
                var bugunSporlar = await _context.WorkoutLogs
                    .Where(w => w.UserId == userId && w.Date.Date == DateTime.Today)
                    .ToListAsync();
                    
                if (bugunSporlar != null && bugunSporlar.Count > 0)
                {
                    model.CaloriesOut = (int)bugunSporlar.Sum(x => x.CaloriesBurned);
                    model.TotalWorkoutMinutesToday = bugunSporlar.Sum(x => x.DurationMinutes);
                    model.TotalWorkoutsToday = bugunSporlar.Count;
                }
            }
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}