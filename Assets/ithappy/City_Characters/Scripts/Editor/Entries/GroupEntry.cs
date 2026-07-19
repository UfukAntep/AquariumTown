using System.Collections.Generic;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Entries
{
    [CreateAssetMenu(menuName = ToolConfig.ToolName + "/Slot Group Entry", fileName = "SlotGroupEntry", order = 1000)]
    public class GroupEntry : ScriptableObject
    {
        public GroupType Type;
        public BodyType BodyType;
        public Gender Gender;
        public List<GameObject> Variants;

        public int Count => Variants.Count;
    }
}