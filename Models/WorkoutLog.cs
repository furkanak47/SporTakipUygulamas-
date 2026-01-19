using System.ComponentModel.DataAnnotations.Schema;

namespace SporTakip.Models;

public class WorkoutLog
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
    
    // ORTAK ALANLAR
    public double DurationMinutes { get; set; } // Süre (Kalori hesabı için şart)
    public double CaloriesBurned { get; set; }

    // YENİ: AĞIRLIK ANTRENMANI İÇİN ALANLAR
    public int? Sets { get; set; }   // Kaç Set?
    public int? Reps { get; set; }   // Kaç Tekrar?
    public double? Weight { get; set; } // Kaç Kilo?
}