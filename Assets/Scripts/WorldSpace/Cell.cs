using System.Collections.Generic;
using UnityEngine;

public class Cell
{

    public int id;
    public Bounds bounds;
    public List<PlayerEntity> entities;

    public Cell(int id, Vector3 center, Vector3 size)
    {
        this.id = id;
        this.entities = new List<PlayerEntity>();
        this.bounds = new Bounds(center, size);
    }

    public void RemoveEnity(PlayerEntity entity)
    {
        if (entities.Contains(entity))
        {
            entities.Remove(entity);
        }
    }

    public void AddEntity(PlayerEntity entity)
    {
        if (!entities.Contains(entity))
        {
            entities.Add(entity);
        }
    }

}
