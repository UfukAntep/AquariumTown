using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class AccessoriesStep : SlotStepBase
    {
        protected override SlotType SlotType => SlotType.Accessories;
        protected override float Probability => .2f;
    }
}