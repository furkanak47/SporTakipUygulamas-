using Microsoft.AspNetCore.Identity;

namespace SporTakip.Models;

public class ApplicationUser : IdentityUser
{
    // Mevcut olanlar:
    public int Age { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public string Gender { get; set; } = "";
    public double TargetWeight { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // YENİ EKLENECEKLER:
    public string ActivityLevel { get; set; } = "Az"; // Az, Orta, Çok
    public string GoalType { get; set; } = "Koru"; // Ver, Al, Koru
// Seçenekler: Ver, Al, Koru
}