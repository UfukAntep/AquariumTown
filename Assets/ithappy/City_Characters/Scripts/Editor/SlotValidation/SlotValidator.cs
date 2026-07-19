using System.Collections.Generic;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.SlotValidation
{
    public class SlotValidator
    {
        private readonly ISlotValidationRules[] _slotValidationRules =
        {
            new AlwaysOnRule()
        };

        public SlotState[] Validate(SlotState slotState)
        {
            var slotStates = new List<SlotState>();
            foreach (var rule in _slotValidationRules)
            {
                var newSlotStates = rule.Validate(slotState);
                slotStates.AddRange(newSlotStates);
            }

            return slotStates.ToArray();
        }
    }
}