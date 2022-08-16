using System.Collections.Generic;
using System.Text;
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

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(string.Format("id:{0} entities:[", id));
        for (int i = 0; i < entities.Count; i++)
        {
            stringBuilder.Append(string.Format(i == (entities.Count - 1) ? "{0}" : "{0},", entities[i].ID));
        }
        stringBuilder.Append("]");
        return stringBuilder.ToString();
    }

}
