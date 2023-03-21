using UnityEngine;

public class quad
{
  public static void Create(GameObject go, int width, int height)
  {
    MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
    meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

    MeshFilter meshFilter = go.AddComponent<MeshFilter>();

    Mesh mesh = new Mesh();

    Vector3[] vertices = new Vector3[4]
    {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, 0, height),
            new Vector3(width, 0, height)
    };
    mesh.vertices = vertices;

    int[] tris = new int[6]
    {
      // lower left triangle
      0, 2, 1,
      // upper right triangle
      2, 3, 1
    };
    mesh.triangles = tris;

    Vector3[] normals = new Vector3[4]
    {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
    };
    mesh.normals = normals;

    Vector2[] uv = new Vector2[4]
    {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
    };
    mesh.uv = uv;

    meshFilter.mesh = mesh;
  }
}