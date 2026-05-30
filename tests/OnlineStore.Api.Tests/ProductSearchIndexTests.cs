using OnlineStore.Api.Services;

namespace OnlineStore.Api.Tests;

public class ProductSearchIndexTests
{
    private static ProductSearchIndex BuildIndex()
    {
        var idx = new ProductSearchIndex();
        idx.Rebuild(new (int, string)[]
        {
            (1, "Wireless Mouse"),
            (2, "Galaxy S24"),
            (3, "Wired Keyboard"),
            (4, "Wireless Headphones"),
        });
        return idx;
    }

    [Fact]
    public void Suggest_MatchesPrefix_CaseInsensitive()
    {
        var idx = BuildIndex();

        var hits = idx.Suggest("WIRE", 10).Select(h => h.Id).ToHashSet();

        Assert.Equal(new HashSet<int> { 1, 3, 4 }, hits);
    }

    [Fact]
    public void Suggest_RespectsTakeLimit()
    {
        var idx = BuildIndex();

        var hits = idx.Suggest("wire", 2);

        Assert.Equal(2, hits.Count);
    }

    [Fact]
    public void Suggest_ReturnsEmpty_ForUnknownPrefix()
    {
        var idx = BuildIndex();

        var hits = idx.Suggest("xyz", 10);

        Assert.Empty(hits);
    }

    [Fact]
    public void Remove_DropsEntryForProduct()
    {
        var idx = BuildIndex();

        idx.Remove(1);
        var hits = idx.Suggest("wireless", 10).Select(h => h.Id).ToArray();

        Assert.DoesNotContain(1, hits);
    }

}
