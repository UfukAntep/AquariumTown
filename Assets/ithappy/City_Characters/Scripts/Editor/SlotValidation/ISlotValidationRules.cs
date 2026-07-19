using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.SlotValidation
{
    public interface ISlotValidationRules
    {
        SlotState[] Validate(SlotState slotState);
    }
}