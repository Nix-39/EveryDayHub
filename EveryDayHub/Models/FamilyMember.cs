namespace EveryDayHub.Models;

public class FamilyMember
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";   // for an example: "Father", "Mother", "Child", etc.
}