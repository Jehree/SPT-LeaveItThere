using System;
using System.IO;
using UnityEngine;

namespace LeaveItThere.Helpers;

public static class BundleLoader
{
    public static AssetBundle LoadBundle(string bundleName, string relativePath = "bundles")
    {
        string bundlePath = Path.Combine(LITUtils.AssemblyFolderPath, relativePath, bundleName);

        return AssetBundle.LoadFromFile(bundlePath) 
            ?? throw new Exception($"Error loading bundle: {bundlePath}");
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