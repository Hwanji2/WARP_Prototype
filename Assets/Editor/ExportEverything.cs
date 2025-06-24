using UnityEditor;
using System.IO;

public class ExportEverything
{
    [MenuItem("Tools/Export All As UnityPackage")]
    public static void ExportAll()
    {
        string exportPath = "ExportedProject.unitypackage";
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();

        // 필터링: 프로젝트 내 Assets 폴더 아래의 자산만 포함
        var validPaths = System.Array.FindAll(assetPaths, path => path.StartsWith("Assets/"));

        if (validPaths.Length == 0)
        {
            EditorUtility.DisplayDialog("Export", "No assets found to export.", "OK");
            return;
        }

        AssetDatabase.ExportPackage(validPaths, exportPath, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
        EditorUtility.DisplayDialog("Export", "Export completed!\nSaved as: " + exportPath, "OK");

        // Finder/Explorer에서 파일 선택
        EditorUtility.RevealInFinder(exportPath);
    }
}
