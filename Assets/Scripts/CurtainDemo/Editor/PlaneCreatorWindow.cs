
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PlaneCreatorWindow : EditorWindow
{
    private const string EDITOR_TAB_NAME = "Utility";
    private const string WINDOW_TITLE = "Plane Creator";

    private const int WINDOW_WIDTH = 300;
    private const int WINDOW_HEIGHT = 300;

    private const string PLANE_NAME_FORMAT = "plane_{0}";
    private const string PLANE_MESH_EXT = ".asset";

    private static PlaneCreatorWindow s_Instance = null;

    private int m_PlaneWidth = 100;
    private int m_PlaneHeight = 100;
    private int m_PlaneResolution = 1;

    private Vector2 m_Pivot = Vector2.one * 0.5f;

    private Texture2D m_PlaneTexture = null;
    private Material m_TextureMaterial = null;
    private Color m_PlaneColor = Color.white;

    [MenuItem(EDITOR_TAB_NAME + "/" + WINDOW_TITLE)]
    private static PlaneCreatorWindow InitializeWindow()
    {
        s_Instance = EditorWindow.GetWindow<PlaneCreatorWindow>(true, WINDOW_TITLE, true) as PlaneCreatorWindow;
        s_Instance.minSize = new Vector2((float)WINDOW_WIDTH, (float)WINDOW_HEIGHT);
        s_Instance.maxSize = new Vector2((float)WINDOW_WIDTH, (float)WINDOW_HEIGHT);
        s_Instance.Show();

        return s_Instance;
    }

    public void OnEnable()
    {
        // Re-assign editor window instance if in case the window is 
        // left open and there are modifications in the script
        if(s_Instance == null)
        {
            s_Instance = this;
        }
    }

    public void OnDisable() { }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Dimensions", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width", GUILayout.Width(100.0f));
        m_PlaneWidth = EditorGUILayout.IntField(m_PlaneWidth, EditorStyles.numberField);
        m_PlaneWidth = Mathf.Clamp(m_PlaneWidth, 1, int.MaxValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Height", GUILayout.Width(100.0f));
        m_PlaneHeight = EditorGUILayout.IntField(m_PlaneHeight, EditorStyles.numberField);
        m_PlaneHeight = Mathf.Clamp(m_PlaneHeight, 1, int.MaxValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Resolution", GUILayout.Width(100.0f));
        m_PlaneResolution = EditorGUILayout.IntField(m_PlaneResolution, EditorStyles.numberField);
        m_PlaneResolution = Mathf.Clamp(m_PlaneResolution, 1, int.MaxValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pivot", GUILayout.Width(100.0f));
        m_Pivot = EditorGUILayout.Vector2Field("", m_Pivot, GUILayout.Width(183.0f));
        m_Pivot.x = Mathf.Clamp01(m_Pivot.x);
        m_Pivot.y = Mathf.Clamp01(m_Pivot.y);

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Render");

        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Main Texture", GUILayout.Width(100.0f));
        m_PlaneTexture = (Texture2D)EditorGUILayout.ObjectField(m_PlaneTexture, typeof(Texture2D), true);
        EditorGUILayout.EndHorizontal();

        if(EditorGUI.EndChangeCheck())
        {
            string textureName = m_PlaneTexture.name;
            string[] assetGuids = AssetDatabase.FindAssets(textureName + " t:Material");

            if(assetGuids.Length == 1)
            {
                string assetMaterialGuid = assetGuids[0];
                string assetMaterialPath = AssetDatabase.GUIDToAssetPath(assetMaterialGuid);
                m_TextureMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetMaterialPath);
            }
            else if(assetGuids.Length <= 0)
            {
                if(EditorUtility.DisplayDialog("Material Not Found", "No material is found for the texture. Do you want to create one?", "Yes", "No"))
                {
                    m_TextureMaterial = new Material(Shader.Find("Sprites/Default"));
                    m_TextureMaterial.mainTexture = m_PlaneTexture;
                    if(!CreateNewAsset(m_TextureMaterial, textureName, "mat", "Plane Material"))
                    {
                        // Cleanup temporary material instance
                        EditorUtility.UnloadUnusedAssetsImmediate();
                    }
                }
            }
            else
            {
                Debug.Log("Unable to find material. There are too many material of the name specified! [" + textureName + "]");
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Material", GUILayout.Width(100.0f));
        m_TextureMaterial = (Material)EditorGUILayout.ObjectField(m_TextureMaterial, typeof(Material), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Color", GUILayout.Width(100.0f));
        m_PlaneColor = EditorGUILayout.ColorField(m_PlaneColor);
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Reset", EditorStyles.toolbarButton))
        {
            ResetToDefaultValues();
        }

        if(GUILayout.Button("Create", EditorStyles.toolbarButton))
        {
            CreatePlaneGameObject();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void CreatePlaneGameObject()
    {
        Mesh planeMesh = CreatePlaneMesh(m_PlaneWidth, m_PlaneHeight, m_PlaneResolution, m_Pivot);

        if(CreateNewAsset(planeMesh, "plane_mesh_" + planeMesh.GetInstanceID(), "asset", "Plane Mesh"))
        {
            GameObject planeObject = new GameObject(string.Format(PLANE_NAME_FORMAT, planeMesh.GetInstanceID()));

            MeshFilter meshFilter = planeObject.AddComponent<MeshFilter>();
            meshFilter.SetMesh(planeMesh);

            MeshRenderer meshRenderer = planeObject.AddComponent<MeshRenderer>();
            meshRenderer.SetMaterial(m_TextureMaterial);
        }
        else
        {
            // Cleanup temporary mesh instance
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
    }

    private Mesh CreatePlaneMesh(int width, int height, int resolution, Vector2 pivot)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = GetMeshVertices(width, height, resolution, pivot);

        int vertsLen = vertices.Length;
        int[] triangles = GetMeshTriangles(vertsLen);
        Color[] colors = GetMeshColors(m_PlaneColor, vertsLen);
        Vector3[] normals = GetMeshNormals(vertsLen);
        Vector2[] uvs = GetMeshUvs(vertices, m_PlaneWidth, m_PlaneHeight, m_Pivot);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.normals = normals;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    private Vector3[] GetMeshVertices(int width, int height, int resolution, Vector2 pivot)
    {
        int totalVerts = 2 * resolution + 2;
        Vector3[] result = new Vector3[totalVerts];

        int index = 0;
        for (int i = 0; i < totalVerts; i += 2)
        {
            Vector2 anchor = new Vector2(width * pivot.x, height * pivot.y);
            result[i] = new Vector3(index * width / resolution - anchor.x, -anchor.y, 0.0f);
            result[i + 1] = new Vector3(index * width / resolution - anchor.x, anchor.y, 0.0f);
            index++;
        }

        return result;
    }

    private int[] GetMeshTriangles(int vertsLen)
    {
        int vertsCount = vertsLen - 2;
        int trisCount = vertsCount * 3;

        int[] result = new int[trisCount];

        int currentTriangleindex = 0;

        for(int i = 0; i < vertsCount; i++)
        {
            if(i % 2 == 1)
            {
                result[currentTriangleindex] = i;
                result[currentTriangleindex + 1] = i + 1;
                result[currentTriangleindex + 2] = i + 2;
            }
            else
            {
                result[currentTriangleindex] = i + 2;
                result[currentTriangleindex + 1] = i + 1;
                result[currentTriangleindex + 2] = i;
            }

            currentTriangleindex += 3;
        }

        return result;
    }

    private Vector2[] GetMeshUvs(Vector3[] vertices, int width, int height, Vector2 pivot)
    {
        int vertsLen = vertices.Length;
        Vector2[] result = new Vector2[vertsLen];

        for(int i = 0; i < vertsLen; i++)
        {
            Vector2 anchor = new Vector2(width * pivot.x, height * pivot.y);
            result[i] = new Vector2((vertices[i].x + anchor.x) / width, (vertices[i].y + anchor.y) / height);
        }

        return result;
    }

    private Vector3[] GetMeshNormals(int vertsLen)
    {
        Vector3[] result = new Vector3[vertsLen];

        for(int i = 0; i < vertsLen; i++)
        {
            result[i] = Vector3.forward;
        }

        return result;
    }

    private Color[] GetMeshColors(Color color, int vertsLen)
    {
        Color[] result = new Color[vertsLen];

        for(int i = 0; i < vertsLen; i++)
        {
            result[i] = color;
        }

        return result;
    }

    private bool CreateNewAsset(Object asset, string name, string extension, string message)
    {
        AssetDatabase.Refresh();

        if(AssetDatabase.Contains(asset))
        {
            return false;
        }

        string assetName = name + extension;
        string assetFullPath = EditorUtility.SaveFilePanelInProject("Save", name, extension, message);
        assetFullPath.Replace('\\', '/');
        assetFullPath.TrimEnd();

        if(string.IsNullOrEmpty(assetFullPath))
        {
            return false;
        }

        AssetDatabase.CreateAsset(asset, assetFullPath);
        AssetDatabase.SaveAssetIfDirty(asset);
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(asset);
        return true;
    }

    private void ResetToDefaultValues()
    {
        m_PlaneWidth = 100;
        m_PlaneHeight = 100;
        m_PlaneResolution = 1;
        m_Pivot = Vector2.one * 0.5f;

        m_PlaneTexture = null;
        m_TextureMaterial = null;
        m_PlaneColor = Color.white;
    }
}

#endif
