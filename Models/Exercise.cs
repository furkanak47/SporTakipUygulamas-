namespace SporTakip.Models;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = ""; 
    public double CaloriesPerHour { get; set; } // Ortalama yakım
    
    // YENİ: Bu hareketin türü ne? (Kardiyo veya Agirlik)
    public string Category { get; set; } = "Kardiyo"; 
}