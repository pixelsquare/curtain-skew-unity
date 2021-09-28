using UnityEngine;

public static class MeshFilterExtension
{
    public static Mesh GetMesh(this MeshFilter meshFilter)
    {
        return (Application.isPlaying) ? meshFilter.mesh : meshFilter.sharedMesh;
    }

    public static void SetMesh(this MeshFilter meshFilter, Mesh mesh)
    {
        if(Application.isPlaying)
        {
            meshFilter.mesh = mesh; 
        }
        else
        {
            meshFilter.sharedMesh = mesh;
        }
    }
}
