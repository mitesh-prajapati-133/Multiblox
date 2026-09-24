using System.Collections.Generic;

public static class CardRegistry
{
    static Dictionary<int, behaviour> map = new Dictionary<int, behaviour>();

    public static void Register(int id, behaviour b)
    {
        if (!map.ContainsKey(id))
            map.Add(id, b);
        else
            map[id] = b;
    }

    public static void Unregister(int id)
    {
        if (map.ContainsKey(id)) map.Remove(id);
    }

    public static behaviour Get(int id)
    {
        if (map.TryGetValue(id, out behaviour b)) return b;
        return null;
    }

    public static void Clear()
    {
        map.Clear();
    }
}
