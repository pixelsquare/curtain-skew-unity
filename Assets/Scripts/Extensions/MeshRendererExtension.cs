
using UnityEngine;

public static class MeshRendererExtension
{
    public static void SetMaterial(this MeshRenderer meshRenderer, Material material)
    {
        meshRenderer.material = (material != null) ? material : new Material(Shader.Find("Sprites/Default"));
    }
}
