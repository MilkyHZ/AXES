using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder : MonoBehaviour
{
    #region Enum
    public enum ShapeType { Cube, Pyramid, Sphere }
    #endregion

    [Header("Shape Settings")]
    [SerializeField]
    private ShapeType _shape = ShapeType.Cube;
    [SerializeField, Range(6, 32)]
    private int _sphereSegments = 6;
    [SerializeField]
    private Material _mat;
    [SerializeField]
    private Vector3 _pos = Vector3.zero;
    [SerializeField]
    private float _scale = 1f;
    [SerializeField]
    private float _yRot = 1f;
    [SerializeField, Range(1, 100)]
    private float _focalLength = 5f;

    [Header("Gizmos")]
    [SerializeField]
    private bool _drawGizmos = true;
    [SerializeField]
    private bool _showHiddenGizmoEdges = true;

    private Vector3[] _verts;
    private int[][] _faces;
    private Mesh _mesh;
    private readonly List<Vector3> _actualPos = new();
    private readonly List<int> _visibleEdges = new();
    private readonly HashSet<(int, int)> _uniqueEdges = new();

    #region Unity Life Cycle

    private void Awake()
    {
        _mesh = new();
        gameObject.AddComponent<MeshFilter>().mesh = _mesh;
        gameObject.AddComponent<MeshRenderer>().material = _mat;
        BuildShape();
    }

    private void OnValidate()
    {
        BuildShape();
    }

    private void LateUpdate()
    {
        _actualPos.Clear();
        foreach (var point in _verts)
        {
            _actualPos.Add(Project(point));
        }
        _mesh.vertices = _actualPos.ToArray();
        _visibleEdges.Clear();
        _uniqueEdges.Clear();

        foreach (var face in _faces)
        {
            if (!FacesCamera(_actualPos[face[0]], _actualPos[face[1]], _actualPos[face[2]])) continue;

            var edgeCount = face.Length;
            for (int i = 0; i < edgeCount; i++)
            {
                int a = face[i];
                int b = face[(i + 1) % edgeCount];

                var edge = a < b ? (a, b) : (b, a);

                if (_uniqueEdges.Add(edge))
                {
                    _visibleEdges.Add(edge.Item1);
                    _visibleEdges.Add(edge.Item2);
                }
            }
        }
        _mesh.SetIndices(_visibleEdges, MeshTopology.Lines, 0);
        _mesh.RecalculateBounds();
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;
        if (_verts == null || _faces == null) BuildShape();

        var projected = new Vector3[_verts.Length];
        for (int i = 0; i < _verts.Length; i++)
        {
            projected[i] = transform.TransformPoint(Project(_verts[i]));
        }

        Gizmos.color = Color.cyan;
        foreach (var p in projected)
        {
            Gizmos.DrawSphere(p, 0.025f * _scale);
        }

        var drawnVisible = new HashSet<(int, int)>();
        foreach (var face in _faces)
        {
            bool isFront = FacesCamera(
                transform.InverseTransformPoint(projected[face[0]]),
                transform.InverseTransformPoint(projected[face[1]]),
                transform.InverseTransformPoint(projected[face[2]])
            );

            int edgeCount = face.Length;
            for (int i = 0; i < edgeCount; i++)
            {
                int a = face[i];
                int b = face[(i + 1) % edgeCount];
                var edge = a < b ? (a, b) : (b, a);

                if (isFront)
                {
                    if (drawnVisible.Add(edge))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(projected[edge.Item1], projected[edge.Item2]);
                    }
                }
                else if (_showHiddenGizmoEdges && !drawnVisible.Contains(edge))
                {
                    Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
                    Gizmos.DrawLine(projected[edge.Item1], projected[edge.Item2]);
                }
            }
        }
    }

    #endregion

    #region Shape Builder

    private void BuildShape()
    {
        if (_mesh != null) _mesh.Clear();

        switch (_shape)
        {
            case ShapeType.Cube:
                BuildCube();
                break;
            case ShapeType.Pyramid:
                BuildPyramid();
                break;
            case ShapeType.Sphere:
                BuildSphere(_sphereSegments, _sphereSegments);
                break;
        }
    }

    private void BuildCube()
    {
        _verts = new Vector3[]
        {
            new(-1,-1,-1), new(1,-1,-1),    // 0, 1
            new( 1, 1,-1), new(-1,1,-1),    // 2, 3
            new(-1,-1, 1), new(1,-1, 1),    // 4, 5
            new( 1, 1, 1), new(-1,1, 1)     // 6, 7
        };

        _faces = new int[][]
        {
            new[]{0,1,2,3}, new[]{5,4,7,6}, new[]{4,0,3,7},     // counter-clockwise
            new[]{1,5,6,2}, new[]{3,2,6,7}, new[]{0,4,5,1}      // from outside
        };
    }

    private void BuildPyramid()
    {
        _verts = new Vector3[]
        {
            new(-1, -1, -1), // 0: front-left base
            new( 1, -1, -1), // 1: front-right base
            new( 1, -1,  1), // 2: back-right base
            new(-1, -1,  1), // 3: back-left base
            new( 0,  1,  0)  // 4: top apex
        };

        _faces = new int[][]
        {
            new[] { 0, 1, 4 },       // front triangle
            new[] { 1, 2, 4 },       // right triangle
            new[] { 2, 3, 4 },       // back triangle
            new[] { 3, 0, 4 },       // left triangle
            new[] { 0, 3, 2, 1 }     // bottom quad
        };
    }

    private void BuildSphere(int rings, int segments)
    {
        var vertList = new List<Vector3>();
        var faceList = new List<int[]>();

        vertList.Add(Vector3.up);

        for (int r = 1; r < rings; r++)
        {
            float phi = Mathf.PI * r / rings;
            float y = Mathf.Cos(phi);
            float ringRadius = Mathf.Sin(phi);

            for (int s = 0; s < segments; s++)
            {
                float theta = -2f * Mathf.PI * s / segments;
                float x = ringRadius * Mathf.Cos(theta);
                float z = ringRadius * Mathf.Sin(theta);
                vertList.Add(new Vector3(x, y, z));
            }
        }

        int bottomIndex = vertList.Count;
        vertList.Add(Vector3.down);

        for (int s = 0; s < segments; s++)
        {
            int next = (s + 1) % segments;
            faceList.Add(new[] { 0, 1 + next, 1 + s });
        }

        for (int r = 0; r < rings - 2; r++)
        {
            int rowA = 1 + r * segments;
            int rowB = 1 + (r + 1) * segments;

            for (int s = 0; s < segments; s++)
            {
                int next = (s + 1) % segments;
                faceList.Add(new[] { rowA + s, rowA + next, rowB + next, rowB + s });
            }
        }

        int lastRow = 1 + (rings - 2) * segments;
        for (int s = 0; s < segments; s++)
        {
            int next = (s + 1) % segments;
            faceList.Add(new[] { bottomIndex, lastRow + s, lastRow + next });
        }

        _verts = vertList.ToArray();
        _faces = faceList.ToArray();
    }

    #endregion

    #region Helper

    private Vector2 Project(Vector3 point)
    {
        point *= .5f;

        var xRot = point.x * Mathf.Cos(_yRot * Mathf.Deg2Rad) - point.z * Mathf.Sin(_yRot * Mathf.Deg2Rad);
        var zRot = point.x * Mathf.Sin(_yRot * Mathf.Deg2Rad) + point.z * Mathf.Cos(_yRot * Mathf.Deg2Rad);
        point = new Vector3(xRot, point.y, zRot);

        point = new Vector3(point.x + _pos.x, point.y + _pos.y, point.z + _pos.z);

        var p = _focalLength / (_focalLength + point.z);
        return new Vector2(point.x, point.y) * _scale * p;
    }

    private bool FacesCamera(Vector2 a, Vector2 b, Vector2 c) => ((b - a).x * (c - a).y) - ((b - a).y * (c - a).x) > 0;

    #endregion
}