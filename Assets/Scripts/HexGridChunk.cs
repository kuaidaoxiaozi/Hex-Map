using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

    HexCell[] cells;

    HexMesh hexMesh;
    Canvas gridCanvas;


    private void Awake() {
        hexMesh = GetComponentInChildren<HexMesh>();
        gridCanvas = GetComponentInChildren<Canvas>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }

    //void Start() {
    //    hexMesh.Triangulate(cells);
    //}

    void LateUpdate() {
        hexMesh.Triangulate(cells);
        enabled = false;
    }

    public void AddCell(int index, HexCell cell) {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    public void Refresh() {
        enabled = true;
        //hexMesh.Triangulate(cells);
    }

}
