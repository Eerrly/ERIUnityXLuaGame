﻿using System.Collections.Generic;
using UnityEngine;

public static class SpacePartition
{

    public static float spaceX;
    public static float spaceZ;

    public static int cellCountX;
    public static int cellCountZ;

    public static List<Cell> cellList;
    public static Vector3 cellSize;
    public static Vector3 cellStartPos;

    public static void Init(float _spaceX, float _spaceZ, Vector3 _cellSize)
    {
        cellList = new List<Cell>();
        spaceX = _spaceX;
        spaceZ = _spaceZ;
        cellSize = _cellSize;
        GenerateCell();
    }

    public static void GenerateCell()
    {
        cellCountX = (int)(spaceX / cellSize.x);
        cellCountZ = (int)(spaceZ / cellSize.z);
        cellStartPos = new Vector3(-spaceX * 0.5f + cellSize.x * 0.5f, 0, -spaceZ * 0.5f + cellSize.z * 0.5f);
        for (int i = 0; i < cellCountX; i++)
        {
            for (int j = 0; j < cellCountZ; j++)
            {
                Vector3 center = cellStartPos + new Vector3(cellSize.x * i, 0, cellSize.z * j);
                Cell cell = new Cell(i * cellCountX + j, center, cellSize);
                cellList.Add(cell);
            }
        }
    }

    public static int PositionIntoIndex(Vector3 pos)
    {
        float dx = pos.x - (spaceX * -1 * 0.5f);
        float dz = pos.z - (spaceZ * -1 * 0.5f);
        int x = (int)(dx / cellSize.x);
        int z = (int)(dz / cellSize.z);
        return x * cellCountX + z;
    }

    public static void UpdateEntityCell(PlayerEntity entity)
    {
        int index = PositionIntoIndex(MathManager.ToVector3(entity.transform.pos));
        if(index >= cellList.Count)
        {
            return;
        }
        Cell targetCell = cellList[index];
        if (entity.cell != targetCell)
        {
            entity.cell?.RemoveEnity(entity);
            entity.cell = targetCell;
            targetCell.AddEntity(entity);
        }
    }

    public static List<Cell> GetCellList()
    {
        return cellList;
    }

}