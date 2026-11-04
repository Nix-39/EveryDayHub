using System.Text.Json.Serialization;

namespace EveryDayHub.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(WorkShift), "workshift")]
[JsonDerivedType(typeof(Activity), "activity")]
// Note will be added here by Alper in card 3:
// [JsonDerivedType(typeof(Note), "note")]
public abstract class BoardItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public int CreatedById { get; set; }

    // Every subclass must write its own version of this method
    public abstract string Describe();
}