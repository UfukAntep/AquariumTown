using System.Collections.Generic;
using System.Linq;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Character;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Extensions;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps.Impl
{
    public class HairStep : IRandomizerStep
    {
        private readonly IEnumerable<GroupType> _groupTypes = new[] { GroupType.Hairstyle, GroupType.HatHairstyle };

        public StepResult Process(CustomizableCharacter character, CharacterState state, GroupType[] groups)
        {
            var availableGroups = _groupTypes.Where(groups.Contains).ToArray();

            if (!availableGroups.Any())
            {
                return new StepResult(state, groups.Where(g => !_groupTypes.Contains(g)));
            }

            var groupType = groups.Where(_groupTypes.Contains).Random();
            var variantsCount = character.GetVariantsCountInGroup(state.BodyType, state.Gender, groupType);
            var index = Random.Range(0, variantsCount);
            var slotState = new SlotState(SlotType.Hair, groupType, true, index);
            var newState = state.Update(slotState);

            var groupsToRemove = new[] { GroupType.Hairstyle, GroupType.HatHairstyle };
            if (groupType.Equals(GroupType.Hairstyle))
            {
                groupsToRemove = groupsToRemove.Append(GroupType.Hat).ToArray();
            }

            var newGroups = groups.Where(g => !groupsToRemove.Contains(g)).ToArray();

            return new StepResult(newState, newGroups);
        }
    }
}