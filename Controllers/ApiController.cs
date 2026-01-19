using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;
using SporTakip.Services;

namespace SporTakip.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICalorieService _calorieService;

    public ApiController(ApplicationDbContext context, ICalorieService calorieService)
    {
        _context = context;
        _calorieService = calorieService;
    }

    // Kullanıcının bugünkü kalori ve makro verilerini JSON olarak döndür
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Bugünkü yemekler
        var todayMeals = await _context.MealLogs
            .Include(m => m.Food)
            .Where(m => m.UserId == userId && m.Date.Date == DateTime.Today)
            .ToListAsync();

        var caloriesIn = todayMeals.Sum(x => (x.Food!.CaloriesPer100g * x.QuantityInGrams) / 100);
        var proteinIn = todayMeals.Sum(x => (x.Food!.ProteinPer100g * x.QuantityInGrams) / 100);
        var fatIn = todayMeals.Sum(x => (x.Food!.FatPer100g * x.QuantityInGrams) / 100);
        var carbsIn = todayMeals.Sum(x => (x.Food!.CarbsPer100g * x.QuantityInGrams) / 100);

        // Bugünkü antrenmanlar
        var todayWorkouts = await _context.WorkoutLogs
            .Where(w => w.UserId == userId && w.Date.Date == DateTime.Today)
            .ToListAsync();

        var caloriesOut = todayWorkouts.Sum(x => x.CaloriesBurned);

        // Hedefler
        var plan = _calorieService.GetPlanForUser(user);
        
        return Ok(new
        {
            caloriesIn = Math.Round(caloriesIn, 0),
            caloriesOut = Math.Round(caloriesOut, 0),
            netCalories = Math.Round(caloriesIn - caloriesOut, 0),
            protein = Math.Round(proteinIn, 1),
            fat = Math.Round(fatIn, 1),
            carbs = Math.Round(carbsIn, 1),
            targetCalories = plan?.TargetCalories ?? 0,
            targetProtein = plan?.TargetProteinGrams ?? 0,
            targetFat = plan?.TargetFatGrams ?? 0,
            targetCarbs = plan?.TargetCarbGrams ?? 0,
            workoutMinutes = todayWorkouts.Sum(x => x.DurationMinutes),
            workoutCount = todayWorkouts.Count
        });
    }

    // Son 7 günün kalori verilerini grafik için döndür
    [HttpGet("weekly-calories")]
    public async Task<IActionResult> GetWeeklyCalories()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var weeklyData = new List<object>();
        
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            var meals = await _context.MealLogs
                .Include(m => m.Food)
                .Where(m => m.UserId == userId && m.Date.Date == date.Date)
                .ToListAsync();
            
            var workouts = await _context.WorkoutLogs
                .Where(w => w.UserId == userId && w.Date.Date == date.Date)
                .ToListAsync();

            var caloriesIn = meals.Sum(x => (x.Food!.CaloriesPer100g * x.QuantityInGrams) / 100);
            var caloriesOut = workouts.Sum(x => x.CaloriesBurned);

            weeklyData.Add(new
            {
                date = date.ToString("dd MMM"),
                caloriesIn = Math.Round(caloriesIn, 0),
                caloriesOut = Math.Round(caloriesOut, 0),
                netCalories = Math.Round(caloriesIn - caloriesOut, 0)
            });
        }

        return Ok(weeklyData);
    }

    // Son 30 günün kilo takibi verilerini döndür
    [HttpGet("weight-progress")]
    public async Task<IActionResult> GetWeightProgress()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var progress = await _context.UserProgresses
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Date)
            .Select(p => new
            {
                date = p.Date.ToString("yyyy-MM-dd"),
                weight = p.Weight
            })
            .ToListAsync();

        return Ok(progress);
    }
}

