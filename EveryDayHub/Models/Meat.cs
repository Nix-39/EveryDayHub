
namespace EveryDayHub.Models
{
    // Represents meat products stored in the fridge
    public class Meat : FridgeItem
    {
        // Warn one day before expiration
        public override int WarningDays => 1;
    }
}
