namespace SporTakip.Models;

public class WorkoutProgram
{
    public int Id { get; set; }
    public string Name { get; set; } = ""; // Örn: Pazartesi (Göğüs)
    public string UserId { get; set; } = ""; // Hangi kullanıcının programı?
    
    public List<WorkoutProgramItem> Items { get; set; } = new();
}