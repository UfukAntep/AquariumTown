using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ithappy.City_Characters.CharacterCustomizationTool.Editor
{
    public static class ScriptableObjectFinder
    {
        public static T FindInstanceByPath<T>(string path) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path is null or empty");
                return null;
            }
            
            if (!AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path))
            {
                Debug.LogError($"Asset not found at path: {path}");
                return null;
            }
        
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T FindInstanceOfType<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            if (guids.Length == 0)
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids.First());

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}