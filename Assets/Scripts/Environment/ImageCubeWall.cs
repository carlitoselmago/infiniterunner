
using UnityEngine;

public class ImageCubeWall : MonoBehaviour
{
    [Header("Image")]
    public Material wallMaterial;
    public Material sideMaterial;

    [Header("Grid")]
    public int columns = 36;
    public int rows = 24;
    public float cubeSize = 0.25f;

    [Header("Destruction")]
    public float breakRadius = 2f;
    public float explosionForce = 5f;
    public float upwardModifier = 0.5f;

    private Rigidbody[] cubes;
    private Transform generatedRoot;

    private void Start()
    {
        GenerateWall();
    }

    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        ClearWall();

        if (wallMaterial == null || wallMaterial.mainTexture == null)
        {
            Debug.LogError("Assign a material with a source texture.");
            return;
        }

        if (columns < 1 || rows < 1 || cubeSize <= 0)
        {
            Debug.LogError("Invalid grid dimensions or cube size.");
            return;
        }

        Texture texture = wallMaterial.mainTexture;

        // Create a dedicated parent for generated cubes
        GameObject root = new GameObject("GeneratedCubes");
        generatedRoot = root.transform;
        generatedRoot.SetParent(transform, false);

        cubes = new Rigidbody[columns * rows];

        // Texture sampling inset
        float du = 0.5f / texture.width;
        float dv = 0.5f / texture.height;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // UV boundaries for this image fragment
                float uMin = (float)x / columns + du;
                float uMax = (float)(x + 1) / columns - du;

                float vMin = 1f - (float)(y + 1) / rows + dv;
                float vMax = 1f - (float)y / rows - dv;

                // Create cube
                GameObject cube = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                cube.name = $"Cube_{x}_{y}";
                cube.transform.SetParent(generatedRoot, false);

                // Flat frontal positioning
                cube.transform.localPosition = new Vector3(
                    (x - (columns - 1) * 0.5f) * cubeSize,
                    ((rows - 1) * 0.5f - y) * cubeSize,
                    0f
                );

                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = Vector3.one * cubeSize;

                // Assign custom mesh
                MeshFilter filter = cube.GetComponent<MeshFilter>();

                filter.sharedMesh = CreateImageCubeMesh(
                    uMin, uMax, vMin, vMax
                );

                // Front and side materials
                MeshRenderer renderer =
                    cube.GetComponent<MeshRenderer>();

                renderer.sharedMaterials = new Material[]
                {
                    wallMaterial,
                    sideMaterial != null ? sideMaterial : wallMaterial
                };

                // Physics
                Rigidbody rb = cube.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                ImageCubePiece piece =
                    cube.AddComponent<ImageCubePiece>();

                piece.Initialize(this);

                cubes[y * columns + x] = rb;
            }
        }

        Debug.Log($"Generated {cubes.Length} image cubes.");
    }

    private Mesh CreateImageCubeMesh(
        float uMin, float uMax,
        float vMin, float vMax)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ImageCubeMesh";

        Vector3[] vertices = new Vector3[24];
        Vector2[] uv = new Vector2[24];

        int[] frontTriangles = new int[6];
        int[] sideTriangles = new int[30];

        int vertexIndex = 0;
        int sideIndex = 0;

        void AddQuad(
            Vector3 a, Vector3 b,
            Vector3 c, Vector3 d,
            Vector2 uvA, Vector2 uvB,
            Vector2 uvC, Vector2 uvD,
            bool isFront)
        {
            int i = vertexIndex;

            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;
            vertices[i + 3] = d;

            uv[i] = uvA;
            uv[i + 1] = uvB;
            uv[i + 2] = uvC;
            uv[i + 3] = uvD;

            int[] triangles =
            {
                i, i + 1, i + 2,
                i, i + 2, i + 3
            };

            if (isFront)
            {
                System.Array.Copy(
                    triangles, 0,
                    frontTriangles, 0, 6
                );
            }
            else
            {
                System.Array.Copy(
                    triangles, 0,
                    sideTriangles, sideIndex, 6
                );

                sideIndex += 6;
            }

            vertexIndex += 4;
        }

        float h = 0.5f;

        // FRONT: negative Z
        AddQuad(
            new Vector3(-h, -h, -h),
            new Vector3(-h, h, -h),
            new Vector3(h, h, -h),
            new Vector3(h, -h, -h),

            new Vector2(uMin, vMin),
            new Vector2(uMin, vMax),
            new Vector2(uMax, vMax),
            new Vector2(uMax, vMin),

            true
        );

        // BACK
        AddQuad(
            new Vector3(-h, -h, h),
            new Vector3(h, -h, h),
            new Vector3(h, h, h),
            new Vector3(-h, h, h),
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.zero, false
        );

        // LEFT
        AddQuad(
            new Vector3(-h, -h, -h),
            new Vector3(-h, -h, h),
            new Vector3(-h, h, h),
            new Vector3(-h, h, -h),
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.zero, false
        );

        // RIGHT
        AddQuad(
            new Vector3(h, -h, h),
            new Vector3(h, -h, -h),
            new Vector3(h, h, -h),
            new Vector3(h, h, h),
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.zero, false
        );

        // TOP
        AddQuad(
            new Vector3(-h, h, -h),
            new Vector3(-h, h, h),
            new Vector3(h, h, h),
            new Vector3(h, h, -h),
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.zero, false
        );

        // BOTTOM
        AddQuad(
            new Vector3(-h, -h, h),
            new Vector3(-h, -h, -h),
            new Vector3(h, -h, -h),
            new Vector3(h, -h, h),
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.zero, false
        );

        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = 2;
        mesh.SetTriangles(frontTriangles, 0);
        mesh.SetTriangles(sideTriangles, 1);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public void BreakAt(Vector3 impactPoint)
    {
        if (cubes == null)
            return;

        foreach (Rigidbody rb in cubes)
        {
            if (rb == null || !rb.isKinematic)
                continue;

            float distance = Vector3.Distance(
                rb.worldCenterOfMass,
                impactPoint
            );

            if (distance <= breakRadius)
            {
                rb.isKinematic = false;

                rb.AddExplosionForce(
                    explosionForce,
                    impactPoint,
                    breakRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );
            }
        }
    }

    [ContextMenu("Clear Wall")]
    public void ClearWall()
    {
        if (generatedRoot == null)
            return;

        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedRoot.GetChild(i);

            MeshFilter filter = child.GetComponent<MeshFilter>();

            if (filter != null && filter.sharedMesh != null)
            {
                DestroyObject(filter.sharedMesh);
            }

            DestroyObject(child.gameObject);
        }

        DestroyObject(generatedRoot.gameObject);

        generatedRoot = null;
        cubes = null;
    }

    private void DestroyObject(Object obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
}
