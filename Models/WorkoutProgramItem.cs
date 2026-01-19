namespace SporTakip.Models;

public class WorkoutProgramItem
{
    public int Id { get; set; }
    
    public int WorkoutProgramId { get; set; }
    public WorkoutProgram? WorkoutProgram { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    // Varsayılan Hedefler
    public int DefaultSets { get; set; } = 3;
    public int DefaultReps { get; set; } = 10;
}