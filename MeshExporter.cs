using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshExporter : MonoBehaviour
{

#if UNITY_EDITOR
    [MenuItem("CONTEXT/MeshFilter/Save .asset")]
    public static void SaveFromInspector(MenuCommand menuCommand)
    {
        var meshFilter = menuCommand.context as MeshFilter;
        if (meshFilter != null)
        {
            var mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                // �ۑ�����p�X���w�肷��
                var path = string.Format($"Assets/SyoFolder/Meshes/{mesh.name}.asset");
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}
