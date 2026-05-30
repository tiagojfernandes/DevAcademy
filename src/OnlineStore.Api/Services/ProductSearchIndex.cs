namespace OnlineStore.Api.Services;

public sealed class ProductSearchIndex
{
    private readonly List<(string NameLower, int Id, string Name)> _sorted = new();

    public void Insert(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        Remove(id); // if it already exists, replace it

        var entry = (name.ToLowerInvariant(), id, name);
        var idx = _sorted.BinarySearch(entry);
        if (idx < 0) idx = ~idx;   // not found -> insertion point (https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.binarysearch?view=net-10.0)
        _sorted.Insert(idx, entry);
    }

    public void Remove(int id)
    {
        var idx = _sorted.FindIndex(e => e.Id == id);
        if (idx >= 0) _sorted.RemoveAt(idx);
    }

    // Rebuild the index at the start and when admin updates products
    public void Rebuild(IEnumerable<(int Id, string Name)> products)
    {
        _sorted.Clear();
        foreach (var (id, name) in products)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            var entry = (name.ToLowerInvariant(), id, name);
            var idx = _sorted.BinarySearch(entry);
            if (idx < 0) idx = ~idx;
            _sorted.Insert(idx, entry);
        }
    }

    public IReadOnlyList<(int Id, string Name)> Suggest(string prefix, int take)
    {
        if (string.IsNullOrWhiteSpace(prefix) || take <= 0)
            return Array.Empty<(int, string)>();

        var p = prefix.Trim().ToLowerInvariant();
        var results = new List<(int Id, string Name)>(take);

        // Binary-search
        var idx = _sorted.BinarySearch((p, 0, ""));
        if (idx < 0) idx = ~idx;

        // return all names that still start with the prefix.
        for (var i = idx; i < _sorted.Count && results.Count < take; i++)
        {
            if (!_sorted[i].NameLower.StartsWith(p)) break;
            results.Add((_sorted[i].Id, _sorted[i].Name));
        }

        return results;
    }
}
