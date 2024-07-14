using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphData
{
    public List<NodeData> Nodes = new List<NodeData>();
}

[Serializable]
public class NodeData
{
    public int ID;
    public SerializableVector3 Position;

    public NodeData(int id, Vector3 position)
    {
        ID = id;
        Position = new SerializableVector3(position);
    }
}


[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public SerializableVector3(Vector3 v3)
    {
        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
