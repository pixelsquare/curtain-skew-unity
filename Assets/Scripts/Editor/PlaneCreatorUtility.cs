
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;

[CreateAssetMenu(fileName = "Plane Creator", menuName = "Plane Creator Utility")]
public class PlaneCreatorUtility : ScriptableObject
{
    private static PlaneCreatorUtility s_Instance = null;
    public static PlaneCreatorUtility Instance
    {
        get
        {
            if(s_Instance == null)
            {
                string instanceName = typeof(PlaneCreatorUtility).Name;

                s_Instance = Resources.Load(instanceName) as PlaneCreatorUtility;

                if(s_Instance == null)
                {
#if UNITY_EDITOR
                    string resourcesRelativePath = Path.Combine("Assets", "Resources");
                    string resourcesFullPath = Path.GetFullPath(resourcesRelativePath);

                    if(!Directory.Exists(resourcesFullPath))
                    {
                        Directory.CreateDirectory(resourcesFullPath);
                    }

                    s_Instance = ScriptableObject.CreateInstance<PlaneCreatorUtility>();
                    AssetDatabase.CreateAsset(s_Instance, resourcesRelativePath + "/" + instanceName + ".asset");
                    AssetDatabase.Refresh();

                    EditorGUIUtility.PingObject(s_Instance);
                    Selection.activeObject = s_Instance;
#endif
                }
            }

            return s_Instance;
        }
    }

    public void Initialize()
    {

    }
}
