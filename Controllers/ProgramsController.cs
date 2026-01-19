using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporTakip.Data;
using SporTakip.Models;

namespace SporTakip.Controllers;

[Authorize]
public class ProgramsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProgramsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. PROGRAMLARIMI LİSTELE
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var programs = await _context.WorkoutPrograms
            .Include(p => p.Items)
            .ThenInclude(i => i.Exercise)
            .Where(p => p.UserId == userId)
            .ToListAsync();
        
        return View(programs);
    }

    // 2. YENİ PROGRAM OLUŞTUR (Sadece İsim)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var program = new WorkoutProgram
            {
                Name = name,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _context.Add(program);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // 3. PROGRAMA HAREKET EKLE
    public async Task<IActionResult> Details(int id)
    {
        var program = await _context.WorkoutPrograms
            .Include(p => p.Items)
            .ThenInclude(i => i.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program == null) return NotFound();

        ViewBag.ExerciseList = _context.Exercises.OrderBy(x => x.Name).ToList();
        return View(program);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int programId, int exerciseId, int sets, int reps)
    {
        var item = new WorkoutProgramItem
        {
            WorkoutProgramId = programId,
            ExerciseId = exerciseId,
            DefaultSets = sets,
            DefaultReps = reps
        };
        _context.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = programId });
    }

    // 4. SİHİRLİ TUŞ: PROGRAMI GÜNLÜĞE AKTAR
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyToLog(int id)
    {
        var program = await _context.WorkoutPrograms
            .Include(p => p.Items)
            .ThenInclude(i => i.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program != null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            foreach (var item in program.Items)
            {
                // Her bir şablonu, bugünün günlüğüne kopyala
                var log = new WorkoutLog
                {
                    UserId = userId,
                    ExerciseId = item.ExerciseId,
                    Date = DateTime.Now,
                    DurationMinutes = 0, // Ağırlık antrenmanı varsayıyoruz
                    Sets = item.DefaultSets,
                    Reps = item.DefaultReps,
                    Weight = 0, // Kilo o gün girilir
                    CaloriesBurned = (item.Exercise.Category == "Agirlik" ? 40 : 100) // Tahmini bir değer
                };
                _context.Add(log);
            }
            await _context.SaveChangesAsync();
        }

        // İşlem bitince Antrenmanlarım sayfasına git
        return RedirectToAction("Index", "Workouts");
    }
    
    // 5. SİLME
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var program = await _context.WorkoutPrograms.FindAsync(id);
        if(program != null) { _context.WorkoutPrograms.Remove(program); await _context.SaveChangesAsync(); }
        return RedirectToAction("Index");
    }

    // 6. HAZIR PROGRAM ŞABLONLARI (Full Body, İtiş/Çekiş/Bacak vb.)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTemplate(string template)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        // Önce kullanıcının var olan programlarını silmiyoruz; yanına ekler.
        // Egzersizleri isimden çekiyoruz (DbSeeder'daki isimlerle eşleşmeli)
        var allExercises = await _context.Exercises.ToListAsync();

        IEnumerable<WorkoutProgram> programsToAdd = template switch
        {
            "FullBody3" => CreateFullBodyTemplate(userId, allExercises),
            "PushPullLegs" => CreatePushPullLegsTemplate(userId, allExercises),
            "UpperLower4" => CreateUpperLowerTemplate(userId, allExercises),
            _ => Array.Empty<WorkoutProgram>()
        };

        if (!programsToAdd.Any())
        {
            return RedirectToAction("Index");
        }

        await _context.WorkoutPrograms.AddRangeAsync(programsToAdd);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    private static WorkoutProgram[] CreateFullBodyTemplate(string userId, List<Exercise> exercises)
    {
        Exercise? Find(string name) => exercises.FirstOrDefault(e => e.Name == name);

        var fullBodyA = new WorkoutProgram
        {
            Name = "Full Body A (Haftada 3 Gün)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        fullBodyA.Items.AddRange(new[] {
            CreateItem(fullBodyA, Find("Squat (Bacak)"), 3, 8),
            CreateItem(fullBodyA, Find("Bench Press (Göğüs)"), 3, 8),
            CreateItem(fullBodyA, Find("Deadlift (Sırt/Bacak)"), 3, 5),
            CreateItem(fullBodyA, Find("Shoulder Press (Omuz)"), 3, 10),
            CreateItem(fullBodyA, Find("Bicep Curl (Kol)"), 3, 12),
            CreateItem(fullBodyA, Find("Tricep Extension (Arka Kol)"), 3, 12),
            CreateItem(fullBodyA, Find("Mekik / Crunch (Karın)"), 3, 15)
        }.Where(i => i != null)!);

        return new[] { fullBodyA };
    }

    private static WorkoutProgram[] CreatePushPullLegsTemplate(string userId, List<Exercise> exercises)
    {
        Exercise? Find(string name) => exercises.FirstOrDefault(e => e.Name == name);

        var push = new WorkoutProgram
        {
            Name = "Push (İtiş)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        push.Items.AddRange(new[] {
            CreateItem(push, Find("Bench Press (Göğüs)"), 4, 8),
            CreateItem(push, Find("Shoulder Press (Omuz)"), 4, 10),
            CreateItem(push, Find("Tricep Extension (Arka Kol)"), 3, 12),
            CreateItem(push, Find("Mekik / Crunch (Karın)"), 3, 15)
        }.Where(i => i != null)!);

        var pull = new WorkoutProgram
        {
            Name = "Pull (Çekiş)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        pull.Items.AddRange(new[] {
            CreateItem(pull, Find("Deadlift (Sırt/Bacak)"), 4, 5),
            CreateItem(pull, Find("Lat Pulldown (Sırt)"), 4, 10),
            CreateItem(pull, Find("Bicep Curl (Kol)"), 3, 12)
        }.Where(i => i != null)!);

        var legs = new WorkoutProgram
        {
            Name = "Legs (Bacak)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        legs.Items.AddRange(new[] {
            CreateItem(legs, Find("Squat (Bacak)"), 4, 8),
            CreateItem(legs, Find("Deadlift (Sırt/Bacak)"), 3, 5),
            CreateItem(legs, Find("Yürüyüş (Hafif)"), 1, 15) // Isınma / Soğuma
        }.Where(i => i != null)!);

        return new[] { push, pull, legs };
    }

    private static WorkoutProgram[] CreateUpperLowerTemplate(string userId, List<Exercise> exercises)
    {
        Exercise? Find(string name) => exercises.FirstOrDefault(e => e.Name == name);

        var upper = new WorkoutProgram
        {
            Name = "Upper (Üst Vücut)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        upper.Items.AddRange(new[] {
            CreateItem(upper, Find("Bench Press (Göğüs)"), 4, 8),
            CreateItem(upper, Find("Shoulder Press (Omuz)"), 4, 10),
            CreateItem(upper, Find("Lat Pulldown (Sırt)"), 4, 10),
            CreateItem(upper, Find("Bicep Curl (Kol)"), 3, 12),
            CreateItem(upper, Find("Tricep Extension (Arka Kol)"), 3, 12)
        }.Where(i => i != null)!);

        var lower = new WorkoutProgram
        {
            Name = "Lower (Alt Vücut)",
            UserId = userId,
            Items = new List<WorkoutProgramItem>()
        };
        lower.Items.AddRange(new[] {
            CreateItem(lower, Find("Squat (Bacak)"), 4, 8),
            CreateItem(lower, Find("Deadlift (Sırt/Bacak)"), 3, 5),
            CreateItem(lower, Find("Yürüyüş (Hafif)"), 1, 15),
            CreateItem(lower, Find("Mekik / Crunch (Karın)"), 3, 15)
        }.Where(i => i != null)!);

        return new[] { upper, lower };
    }

    private static WorkoutProgramItem? CreateItem(WorkoutProgram program, Exercise? exercise, int sets, int reps)
    {
        if (exercise == null) return null;

        return new WorkoutProgramItem
        {
            WorkoutProgram = program,
            Exercise = exercise,
            ExerciseId = exercise.Id,
            DefaultSets = sets,
            DefaultReps = reps
        };
    }
}