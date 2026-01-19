using System.ComponentModel.DataAnnotations.Schema;

namespace SporTakip.Models;

public class MealLog
{
    public int Id { get; set; }
    
    // Hangi Kullanıcı Yedi?
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    // Hangi Yemeği Yedi?
    public int FoodId { get; set; }
    public Food? Food { get; set; }

    // Ne Zaman ve Ne Kadar?
    public DateTime Date { get; set; } = DateTime.Now; // Tarih
    public double QuantityInGrams { get; set; } // Kaç gram yedi?
    public string MealType { get; set; } = "Öğle"; // Kahvaltı, Öğle, Akşam, Ara Öğün
}