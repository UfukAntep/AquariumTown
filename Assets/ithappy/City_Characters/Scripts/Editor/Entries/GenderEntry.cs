using System.Collections.Generic;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Entries
{
    [CreateAssetMenu(menuName = ToolConfig.ToolName + "/Gender Entry", fileName = "GenderEntry", order = 2)]
    public class GenderEntry : ScriptableObject
    {
        public BodyType BodyType;
        public Gender Gender;
        public List<SlotEntry> Slots;
        public List<FullBodyEntry> FullBodyEntries;
    }
}