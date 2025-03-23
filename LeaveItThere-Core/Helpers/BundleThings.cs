using System;
using System.IO;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    public class BundleThings
    {
        public static GameObject MoveModeUIPrefab = null;

        public static void LoadBundles()
        {
            var bundlePath = Path.Combine(Plugin.AssemblyFolderPath, "bundles", "editplaceditemmenu.menu");
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                throw new Exception($"Error loading bundle: {bundlePath}");
            }

            MoveModeUIPrefab = LoadAsset<GameObject>(bundle, "EditPlacedItemMenu");
        }

        public static T LoadAsset<T>(AssetBundle bundle, string assetPath) where T : UnityEngine.Object
        {
            T asset = bundle.LoadAsset<T>(assetPath);

            if (asset == null)
            {
                throw new Exception($"Error loading asset {assetPath}");
            }

            GameObject.DontDestroyOnLoad(asset);
            return asset;
        }
    }
}
