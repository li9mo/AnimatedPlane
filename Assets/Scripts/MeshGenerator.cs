using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using Unity.Jobs;
using Unity.Burst;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Shader _wireframeShader;
    [SerializeField] private Shader _lightingShader;
    [SerializeField] private int _sizeX;
    [SerializeField] private int _sizeY;
    [SerializeField] private float _strenght = 10;
    private bool _isWorking = false;
    private Mesh _mesh;
    private Material _wireframeMaterial;
    private Material _lightingMaterial;
    private bool _isWireframeViewActive = true;
    private MeshRenderer _meshRenderer;
    NativeArray<Vector3> _vertices;

    void Start()
    {
        CreateMaterials();
        GenerateMesh();
    }

    private void CreateMaterials()
    {
        _wireframeMaterial = new Material(_wireframeShader);
        _lightingMaterial = new Material(_lightingShader);
    }

    private void GenerateMesh()
    {
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        _vertices = new NativeArray<Vector3>(_sizeY*_sizeX, Allocator.Persistent);
        // Set vertices
        for (int i = 0; i < _sizeX; i++)
        {
            for (int j = 0; j < _sizeY; j++)
            {
                _vertices[i+j]=(new Vector3(i, 0, j));
                colors.Add(new Color(0.851f, 0.894f, 0.984f));
            }
        }
        // Set triangles
        for (int i = 0; i < _sizeX - 1; i++)
        {
            for (int j = 0; j < _sizeY - 1; j++)
            {
                // First triangle in square 
                triangles.Add((i * _sizeY) + j);
                triangles.Add((i * _sizeY) + j + 1);
                triangles.Add(((i + 1) * _sizeY) + j);
                // Second triangle in square
                triangles.Add((i * _sizeY) + j + 1);
                triangles.Add(((i + 1) * _sizeY) + j + 1);
                triangles.Add(((i + 1) * _sizeY) + j);
            }
        }

        _mesh = new Mesh();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetColors(colors);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        var meshFillter = GetComponent<MeshFilter>();
        if (meshFillter == null)
        {
            meshFillter = gameObject.AddComponent<MeshFilter>();
        }

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        meshFillter.mesh = _mesh;
        _meshRenderer.material = _wireframeMaterial;
        _isWorking = true;
    }

    private void FixedUpdate()
    {
        if (_isWorking)
        {
            var newVertexPositionJob = new CalculateNewVertexPosition()
            {
                SizeX = _sizeX,
                SizeY = _sizeY,
                Strenght = _strenght,
                Vertices = _vertices,
                TimeChange = Time.realtimeSinceStartup,
            };

            newVertexPositionJob.Schedule(_vertices.Length,30).Complete();
            _mesh.SetVertices(_vertices);
            if (!_isWireframeViewActive)
            {
               _mesh.RecalculateNormals();
            }
        }
    }

    private struct CalculateNewVertexPosition : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<Vector3> Vertices;
        public int SizeX;
        public int SizeY;
        public float Strenght;
        public float TimeChange;

        public void Execute(int i)
        {
            int y = i % SizeY;
            int x = (i - y) / SizeY;

            float yValue = Sin(PI * ((float)x / (float)SizeX + (float)y / (float)SizeY) + TimeChange) * Strenght;
            Vertices[i] = new Vector3(x, yValue, y);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            _isWireframeViewActive = !_isWireframeViewActive;
            _meshRenderer.material = _isWireframeViewActive ? _wireframeMaterial : _lightingMaterial;
        }
    }

    private void OnDisable()
    {
        _vertices.Dispose();
    }
}
