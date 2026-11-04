namespace EveryDayHub.Models;
public class Note : BoardItem
{
    public string Text { get; set; } = "";
    public string Author { get; set; } = "";
    public string Type { get; set; } = "";
    public new DateTime Date { get; set; } = DateTime.Now;

    public override string Describe()
    {
        return $"[{Type}] {Text} - {Author} ({Date:yyyy-MM-dd})";
    }
    
}