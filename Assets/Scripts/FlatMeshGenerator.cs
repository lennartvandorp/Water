using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatMeshGenerator : MonoBehaviour
{
    [SerializeField] public int amountOfSquares;
    [SerializeField] float waveSpeed;
    [SerializeField] float accelerationUpdateTime;

    WaterVerticesList waterVerticesList;

    MeshFilter meshFilter;
    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        GenerateSquare(amountOfSquares);
        meshFilter.mesh = mesh;
        waterVerticesList = new WaterVerticesList(meshFilter.mesh.vertices, amountOfSquares);
        mesh.RecalculateNormals();
    }

    float timer = 0f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            waterVerticesList.AccelerateVertex(amountOfSquares + 2, 1f, .1f);
        }

        timer += Time.deltaTime;
        if (accelerationUpdateTime <= timer)
        {
            LevelOut(timer);
            timer = 0f;
        }
        waterVerticesList.UpdateVertices();

        mesh.vertices = waterVerticesList.GetVertices();
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Creates a 1:1 square
    /// </summary>
    /// <param name="density">The width and height in amount of sub-squares</param>
    void GenerateSquare(int density)
    {
        Vector3[] newVertices = new Vector3[density * density];
        List<int> newTriangles = new List<int>();

        for (int y = 0; y < density; y++)
        {
            for (int x = 0; x < density; x++)
            {
                newVertices[y * density + x] =
                    new Vector3(1f / (density - 1) * x, 0f, 1f / (density - 1) * y) +
                    new Vector3(-.5f, 0f, -.5f);
                if (y < density - 1 && x < density - 1)
                {
                    int[] newSquare = MakeSquare(y * density + x, density);
                    for (int i = 0; i < newSquare.Length; i++)
                    {
                        newTriangles.Add(newSquare[i]);
                    }
                }
            }
        }
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles.ToArray();

    }

    /// <summary>
    /// Creates a subsquare
    /// </summary>
    /// <param name="leftUpper"></param>
    /// <param name="density"></param>
    /// <returns></returns>
    int[] MakeSquare(int leftUpper, int density)
    {
        return new int[] { leftUpper + 1, leftUpper, leftUpper + density, leftUpper + density + 1, leftUpper + 1, leftUpper + density };
    }

    void LevelOut(float timeSinceLast)
    {
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            if (i + 2 < mesh.triangles.Length)
            {
                float avgHeight = (verts[mesh.triangles[i]].y + verts[mesh.triangles[i + 1]].y + verts[mesh.triangles[i + 2]].y) / 3;
                waterVerticesList.AccelerateVertex(mesh.triangles[i], avgHeight, timeSinceLast);
                waterVerticesList.AccelerateVertex(mesh.triangles[i + 1], avgHeight, timeSinceLast);
                waterVerticesList.AccelerateVertex(mesh.triangles[i + 2], avgHeight, timeSinceLast);

            }
        }
        mesh.vertices = verts;
    }
}

class WaterVerticesList
{
    int amountOfVectors;
    int density;

    WaterVertex[] waterVertices;


    public WaterVerticesList(Vector3[] vectors, int _density)
    {
        amountOfVectors = vectors.Length;
        density = _density;
        CreateVertices(vectors);
    }

    void CreateVertices(Vector3[] vectors)
    {
        waterVertices = new WaterVertex[amountOfVectors];
        for (int i = 0; i < waterVertices.Length; i++)
        {
            waterVertices[i] = new WaterVertex(vectors[i]);
            //Defines all vertices on the sides of the grid
            if (i < density || i % density == density - 1 || i % density == 0 || i > density * density - density)
            {
                waterVertices[i].SetSideVertex();
            }
        }
    }
    public void UpdateVertices()
    {
        for (int i = 0; i < waterVertices.Length; i++)
        {
            waterVertices[i].Update();

        }
    }

    public Vector3[] GetVertices()
    {
        Vector3[] toReturn = new Vector3[amountOfVectors];

        for (int i = 0; i < waterVertices.Length; i++)
        {
            toReturn[i] = waterVertices[i].Pos;
        }

        return toReturn;
    }

    public void AccelerateVertex(int index, float targetHeight, float timeSinceLast)
    {
        waterVertices[index].Accelerate((targetHeight - waterVertices[index].Pos.y) * timeSinceLast, timeSinceLast);
    }
}

class WaterVertex
{
    float currentVerticalSpeed;
    Vector3 pos;


    public Vector3 Pos { get { return pos; } }

    public WaterVertex(Vector3 _pos)
    {
        pos = _pos;
    }
    public void Update()
    {
        if (!isSideVertex)
        {
            pos += new Vector3(0f, currentVerticalSpeed, 0f);
        }
    }

    public void Accelerate(float amount, float timeSinceLast)
    {
        currentVerticalSpeed += amount;
        float slowDown = .2f;
        currentVerticalSpeed *= 1f - (timeSinceLast * slowDown);
    }

    bool isSideVertex;
    public void SetSideVertex()
    {
        isSideVertex = true;
    }
}
