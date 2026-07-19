using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class FacialHairStep : SlotStepBase
    {
        protected override SlotType SlotType => SlotType.FacialHair;
        protected override float Probability => .2f;
    }
}