namespace HouseRules.Models.DTOS;

public class ChoreDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Difficulty { get; set; }
    public int ChoreFrequencyDays { get; set; }

    // Nav
    public List<ChoreAssignmentDTO> ChoreAssignments { get; set; }
    public List<ChoreCompletionDTO> ChoreCompletions { get; set; }

}