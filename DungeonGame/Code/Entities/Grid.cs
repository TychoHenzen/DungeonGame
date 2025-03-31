#region

using System;

#endregion

namespace DungeonGame.Code.Entities;

public class Grid<T>(int width, int height)
{
    private readonly T[] items = new T[width * height];
    public int Width { get; } = width;
    public int Height { get; } = height;

    public T this[int x, int y]
    {
        get => items[x + y * Width];
        set => items[x + y * Width] = value;
    }

    public void Foreach(Action<int, int> action)
    {
    }
}
