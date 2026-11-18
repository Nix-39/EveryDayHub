namespace EveryDayHub.Models;

// En aktivitet, till exempel fotbollsträning. Ärver Id, Title, Date och CreatedById från BoardItem.
public class Activity : BoardItem
{
    // Var aktiviteten är
    public string Location { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // Id-nummer för alla familjemedlemmar som deltar
    public List<int> ParticipantIds { get; set; } = new();

    // Skriver ut aktiviteten som text, till exempel "Fotboll på Heden, 18:00-19:30"
    public override string Describe() =>
        $"{Title} på {Location}, {Start:HH:mm}-{End:HH:mm}";
}