using ithappy.City_Characters.CharacterCustomizationTool.Editor.Enums;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor.Entries
{
    [CreateAssetMenu(menuName = ToolConfig.ToolName + "/Full Body Entry", fileName = "FullBodyEntry")]
    public class FullBodyEntry : ScriptableObject
    {
        public BodyType BodyType;
        public Gender Gender;
        public GameObject[] Slots;
    }
}