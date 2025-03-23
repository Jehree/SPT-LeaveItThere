using UnityEditor;

public class CreateAssetBundles {
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles() {
        BuildPipeline.BuildAssetBundles("Assets/MoveModeUI/BundleOutput", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}