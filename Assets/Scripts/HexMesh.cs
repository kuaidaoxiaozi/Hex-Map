using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

    Mesh hexMesh;
    List<Vector3> vertices;
    List<Color> colors;
    List<int> triangles;

    MeshCollider meshCollider;

    void Awake() {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();
    }

    public void Triangulate(HexCell[] cells) {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++) {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    void Triangulate(HexCell cell) {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell) {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE) {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }

    void TriangulateConnection(
        HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2
    ) {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null) {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;


        if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        } else {
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);
        }


        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null) {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

            if (cell.Elevation <= neighbor.Elevation) {
                if (cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                } else {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            } else if (neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            } else {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }

            //AddTriangle(v2, v4, v5);
            //AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
        ) {

        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope) {
            if (rightEdgeType == HexEdgeType.Slope) {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            } else if (rightEdgeType == HexEdgeType.Flat) {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                return;
            }
            TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
        }
        if (leftEdgeType == HexEdgeType.Flat) {
            if (rightEdgeType == HexEdgeType.Slope) {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
        }

        if (leftEdgeType == HexEdgeType.Cliff) {
            if (rightEdgeType == HexEdgeType.Slope) {
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }

        //AddTriangle(bottom, left, right);
        //AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
    }


    void TriangulateCornerTerraces(
    Vector3 begin, HexCell beginCell,
    Vector3 left, HexCell leftCell,
    Vector3 right, HexCell rightCell) {

        Vector3 v1 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v2 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c1 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);
        AddTriangle(begin, v1, v2);
        AddTriangleColor(beginCell.color, c1, c2);

        Vector3 v3 = v1;
        Vector3 v4 = v2;
        Color c3 = c1;
        Color c4 = c2;
        for (int i = 2; i <= HexMetrics.terraceSteps; i++) {
            v1 = v3;
            v2 = v4;
            c1 = c3;
            c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }
    }


    void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell) {

        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        Vector3 boundary = Vector3.Lerp(begin, left, b);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

        //Vector3 v2 = begin;
        //Color c2 = beginCell.color;
        //for (int i = 1; i <= HexMetrics.terraceSteps; i++) {
        //    Vector3 v1 = v2;
        //    Color c1 = c2;
        //    v2 = HexMetrics.TerraceLerp(begin, right, i);
        //    c2 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);

        //    AddTriangle(v1, boundary, v2);
        //    AddTriangleColor(c1, boundaryColor, c2);
        //}
        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);




    }




    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell) {

        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        } else {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }


    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor) {

        Vector3 v2 = begin;
        Color c2 = beginCell.color;
        for (int i = 1; i <= HexMetrics.terraceSteps; i++) {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }
    }



    void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell) {

        //Vector3 v11 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        //Vector3 v12 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        ////AddQuad(beginLeft, beginRight, endLeft, endRight);
        //AddQuad(beginLeft, beginRight, v11, v12);
        //AddQuadColor(beginCell.color, endCell.color);

        Vector3 v1 = beginLeft;
        Vector3 v2 = beginRight;
        Vector3 v3 = beginLeft;
        Vector3 v4 = beginRight;
        Color c1 = beginCell.color;
        Color c2 = beginCell.color;

        for (int i = 1; i <= HexMetrics.terraceSteps; i++) {
            v1 = v3;
            v2 = v4;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c1 = c2;
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }

    }







    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor(Color color) {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    void AddTriangleColor(Color c1, Color c2, Color c3) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
}