using System.Linq;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Character;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps
{
    public abstract class StepBase : IRandomizerStep
    {
        protected abstract GroupType GroupType { get; }

        public abstract StepResult Process(CustomizableCharacter character, CharacterState state, GroupType[] groups);

        protected GroupType[] RemoveSelf(GroupType[] groups) => groups.Where(g => g != GroupType).ToArray();
    }
}