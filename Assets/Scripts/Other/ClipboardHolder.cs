using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipboardEntry
{
    public int prefabIndex;
    public Vector2Int relativePos;
}
public static class ClipboardHolder
{
    public static List<ClipboardEntry> clipboard = new List<ClipboardEntry>();
    public static Vector2Int clipboardOrigin;
}