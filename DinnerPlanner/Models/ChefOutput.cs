namespace DinnerPlanner.Models
{
    public class ChefOutput
    {
        public List<string> Ingredients { get; set; }
        public NutritionalValue NutritionalValuePer100g { get; set; }
        public List<InstructionStep> Instructions { get; set; }
    }
}
