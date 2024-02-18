using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TilePosition
{
    public int x;
    public int z;

    public TilePosition(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return "x: " + x + "; z: " + z;
    }

    public static bool operator ==(TilePosition a, TilePosition b)
    {
        return a.x == b.x && a.z == b.z;
    }

    public static bool operator !=(TilePosition a, TilePosition b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is TilePosition position &&
               x == position.x &&
               z == position.z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, z);
    }

    public bool Equals(TilePosition other)
    {
        return this == other;
    }

    public static TilePosition operator +(TilePosition a, TilePosition b)
    {
        return new TilePosition(a.x + b.x, a.z + b.z);
    }

    public static TilePosition operator -(TilePosition a, TilePosition b)
    {
        return new TilePosition(a.x - b.x, a.z - b.z);
    }
}
