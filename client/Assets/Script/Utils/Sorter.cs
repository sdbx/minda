using System;
using Models;

public static class Sorter
{
    public static int SortRoomWithDateTime(Room a, Room b)
    {
        return b.CreatedAt.CompareTo(a.CreatedAt);
    }
}
