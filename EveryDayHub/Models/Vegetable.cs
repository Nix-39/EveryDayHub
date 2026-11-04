
namespace EveryDayHub.Models
{
    // Represents vegetables stored in the fridge
    public class Vegetable : FridgeItem
    {
        // Warn two days before expiration
        public override int WarningDays => 2;
    }
}
