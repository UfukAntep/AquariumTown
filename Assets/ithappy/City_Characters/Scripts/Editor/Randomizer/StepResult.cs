using System.Collections.Generic;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer
{
    public class StepResult
    {
        public CharacterState State { get; }
        public IEnumerable<GroupType> AvailableGroups { get; }

        public StepResult(CharacterState state, IEnumerable<GroupType> availableGroups)
        {
            State = state;
            AvailableGroups = availableGroups;
        }
    }
}