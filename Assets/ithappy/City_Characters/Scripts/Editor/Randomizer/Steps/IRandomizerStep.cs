using ithappy.City_Characters.CharacterCustomizationTool.Editor.Character;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.State;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Randomizer.Steps
{
    public interface IRandomizerStep
    {
        StepResult Process(CustomizableCharacter character, CharacterState state, GroupType[] groups);
    }
}