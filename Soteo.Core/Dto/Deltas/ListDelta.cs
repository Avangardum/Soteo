using System.Collections.Immutable;

namespace Soteo.Core.Dto.Deltas;

public static class ListDelta
{
    public static ListDelta<T> Between<T>(IReadOnlyList<T> from, IReadOnlyList<T> to)
    {
        Dictionary<int, T> changes = [];
        for (int i = 0; i < to.Count; i++)
            if (i >= from.Count || !Equals(from[i], to[i]))
                changes[i] = to[i];
        bool hasChanged = changes.Count > 0 || from.Count != to.Count;
        return new ListDelta<T> { Count = to.Count, Changes = changes, HasChanged = hasChanged };
    }

    public static ListDelta<T> FromNewList<T>(IReadOnlyList<T> list)
    {
        return new ListDelta<T>
        {
            Count = list.Count,
            Changes = list.Select((item, index) => new KeyValuePair<int, T>(index, item)).ToImmutableDictionary(),
            HasChanged = true
        };
    }
}

public sealed class ListDelta<T>
{
    public required int Count { get; init; }
    public IReadOnlyDictionary<int, T> Changes { get; init; } = ImmutableDictionary<int, T>.Empty;
    public required bool HasChanged { get; init; }

    public void MutateList(IList<T> list, double interpolationWeight, Func<T, T, double, T> interpolateValue)
    {
        while (list.Count < Count)
            list.Add(Changes[list.Count]);

        while (list.Count > Count)
            list.RemoveAt(list.Count - 1);

        foreach ((int index, T value) in Changes)
            list[index] = interpolateValue(list[index], value, interpolationWeight);
    }

    public void MutateList(IList<T> list) => MutateList(list, 1, (_, to, _) => to);

    public override string ToString()
    {
        if (!HasChanged) return "Unchanged";
        string changesStr = "{ " + Changes.Select(it => $"[{it.Key}] = {it.Value}").JoinToString(", ") + " }";
        return $$"""ListDelta<{{typeof(T).Name}}>""" +
            $$"""{ Count = {{Count}}, Changes = {{changesStr}}, HasChanged = {{HasChanged}} }""";
    }
}
