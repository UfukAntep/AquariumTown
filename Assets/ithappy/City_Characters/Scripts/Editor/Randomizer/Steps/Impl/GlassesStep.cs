using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class GlassesStep : SlotStepBase
    {
        protected override SlotType SlotType => SlotType.Glasses;
        protected override float Probability => .2f;
    }
}