using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class GlovesStep : SlotStepBase
    {
        protected override SlotType SlotType => SlotType.Gloves;
        protected override float Probability => .1f;
    }
}