﻿using UnityEngine;
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

        //hexMesh.vertices = new Vector3[] {
        //    new Vector3(0, 0, 0), new Vector3(0, 0, 10), new Vector3(10, 0, 0),
        //    new Vector3(10, 0, 0), new Vector3(0, 0, 10), new Vector3(10, 0, 10)
        //};
        //hexMesh.colors = new Color[] {
        //    Color.white, Color.white, Color.white,
        //    Color.black, Color.black, Color.black
        //};
        //hexMesh.triangles = new int[] {
        //    0,1,2,
        //    3,4,5
        //};
        //hexMesh.RecalculateNormals();
        //meshCollider.sharedMesh = hexMesh;
    }

    void Triangulate(HexCell cell) {
        for (HexDirection i = HexDirection.NE; i <= HexDirection.NW; i++) {
            Triangulate(i, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell) {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);
        //AddTriangleColor(
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3,
        //    (cell.color + neighbor.color + nextNeighbor.color) / 3
        //    );

        if (direction <= HexDirection.SE) {
            TriangulateConnection(direction, cell, v1, v2);
        }

        //Vector3 bridge = HexMetrics.GetBridge(direction);
        ////Vector3 v3 = center + HexMetrics.GetFirstCorner(direction);
        ////Vector3 v4 = center + HexMetrics.GetSecondCorner(direction);
        //Vector3 v3 = v1 + bridge;
        //Vector3 v4 = v2 + bridge;

        //AddQuad(v1, v2, v3, v4);

        //HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        //HexCell neighbor = cell.GetNeighbor(direction) ?? cell;
        //HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;

        //Color bridgeColor = (cell.color + neighbor.color) * 0.5f;
        //AddQuadColor(cell.color, bridgeColor);

        ////AddQuadColor(
        ////    cell.color,
        ////    cell.color,
        ////    //(cell.color + prevNeighbor.color + neighbor.color) / 3,
        ////    //(cell.color + neighbor.color + nextNeighbor.color) / 3
        ////    (cell.color + neighbor.color) / 2,
        ////    (cell.color + neighbor.color) / 2
        ////    );


        //AddTriangle(v1, center + HexMetrics.GetFirstCorner(direction), v3);
        //AddTriangleColor(cell.color, (cell.color + prevNeighbor.color + neighbor.color) / 3, bridgeColor);

        //AddTriangle(v2, v4, center + HexMetrics.GetSecondCorner(direction));
        //AddTriangleColor(cell.color, bridgeColor, (cell.color + neighbor.color + nextNeighbor.color) / 3);

    }


    void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {

        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null) {
            return;
        }


        Vector3 b = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + b;
        Vector3 v4 = v2 + b;

        AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbor.color);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (nextNeighbor != null) {
            AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
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

    void AddTriangleColor(Color c1) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c1);
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

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    void AddQuadColor(Color c1, Color c2) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

}