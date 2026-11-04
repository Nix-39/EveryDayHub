namespace EveryDayHub.Models;

public abstract class BoardItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public int CreatedById { get; set; }

    // Every subclass must write its own version of this method
    public abstract string Describe();
}