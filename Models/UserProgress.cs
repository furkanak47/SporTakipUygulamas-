using System.ComponentModel.DataAnnotations.Schema;

namespace SporTakip.Models;

public class UserProgress
{
    public int Id { get; set; }
    
    // Hangi Kullanıcı?
    public string UserId { get; set; } = "";
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public double Weight { get; set; } // O günkü kilosu
    public DateTime Date { get; set; } = DateTime.Now; // Tartıldığı tarih
}