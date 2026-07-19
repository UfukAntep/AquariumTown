using System.Collections.Generic;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Character;
using ithappy.City_Characters.CharacterCustomizationTool.Editor.Entries;
using UnityEditor;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor
{
    [CreateAssetMenu(fileName = "ToolConfig", menuName = "ithappy/City_Characters" + "/Tool Config", order = 0)]
    public class ToolConfig : ScriptableObject
    {
        public const string ToolName = "ithappy/City_Characters";
        public const string ConfigsFolderName = "Configs";

        [SerializeField]
        private string _packageName;
    public const string ToolConfigPath = "Assets/ithappy/City_Characters/Configs/ToolConfig.asset";

        [SerializeField]
        private List<BodyTypeEntry> _bodyTypes;

        private static ToolConfig _instance;
        private static string _rootPath;

        public static string PackageName => Instance._packageName;
        public static string RootPath => string.IsNullOrEmpty(_rootPath) ? _rootPath = BaseMeshAccessor.FindRoot() : _rootPath;
        public static string AnimationController => RootPath + "Animations/Animation_Controllers/Character_Movement.controller";
        public static string SavedCharacters => RootPath + "Saved_Characters/";
        public static string Meshes => RootPath + "Meshes";
        public static string Faces => Meshes + "/Faces/";
        public static string Configs => RootPath + ConfigsFolderName;
        public static string BodyTypes => $"{Configs}/BodyTypes";
        public static List<BodyTypeEntry> BodyTypeEntries => Instance._bodyTypes;

            private static ToolConfig Instance => !_instance ? _instance = ScriptableObjectFinder.FindInstanceByPath<ToolConfig>(ToolConfigPath) : _instance;

        public static void Reload()
        {
            _instance = null;
            _rootPath = null;
        }

        public static void SetBodyTypes(IEnumerable<BodyTypeEntry> bodyTypes)
        {
            Instance._bodyTypes.Clear();
            Instance._bodyTypes.AddRange(bodyTypes);
            EditorUtility.SetDirty(Instance);
        }

        private void OnValidate()
        {
            Reload();
        }
    }
}