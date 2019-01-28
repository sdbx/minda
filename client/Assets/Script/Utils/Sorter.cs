using System;
using Models;

public static class Sorter
{
    public static int SortRoomWithDateTime(Room a, Room b)
    {
        return b.created_at.CompareTo(a);
    }
}