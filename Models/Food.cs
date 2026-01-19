namespace SporTakip.Models;

public class Food
{
    public int Id { get; set; }
    public string Name { get; set; } = ""; // Yemek Adı
    public double CaloriesPer100g { get; set; } // 100gr Kalorisi
    public double ProteinPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public double FatPer100g { get; set; }
    public string? ImageUrl { get; set; } // Yemek Resmi
    public double GramsPerServing { get; set; } = 100;
}