using System.Collections.Generic;
using MarketMakingGame.Shared.Models;
using System.Linq;
using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Server.Lib
{
  public class Bag<T>
  {
    class ItemCount
    {
      public T Item { get; }

      public int Count { get; set; }

      public ItemCount(T card, int count)
      {
        Item = card;
        Count = count;
      }

      public override string ToString()
      {
        return this.ToStringWithProperties();
      }
    }

    private readonly object _lock = new object();
    private List<ItemCount> _items;
    private readonly Random _random;
    private readonly T _defaultValue;

    public Bag(IEnumerable<T> allItems, IEnumerable<T> drawnItems, T defaultValue, int numDecks)
    {
      System.Console.WriteLine($"{numDecks}");
      var dealt = drawnItems.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
      numDecks = Math.Max(1, Math.Max(numDecks, dealt.Values.DefaultIfEmpty(0).Max()));
      System.Console.WriteLine($"{numDecks} {string.Join(",", dealt)}");
      _items = allItems
        .Select(x => new ItemCount(x, numDecks - dealt.GetValueOrDefault(x, 0)))
        .Where(x => x.Count > 0)
        .ToList();
      _items.TrimExcess();

      _random = new Random();
      _defaultValue = defaultValue;
    }

    public T Draw(int maxIter = 100)
    {
      lock (_lock)
      {
        if (_items.Count <= 0)
          return _defaultValue;

        var counter = 0;
        while (++counter < maxIter)
        {
          var index = _random.Next(0, _items.Count);
          var selected = _items[index];
          if (selected.Count > 0)
          {
            selected.Count -= 1;
            return selected.Item;
          }
        }

        var items = _items.Where(x => x.Count > 0).ToList();
        _items.Clear();
        _items.AddRange(items);
      }

      return Draw(maxIter);
    }

    public override string ToString()
    {
      return $"Bag(Items=[{String.Join(",", _items)}])";
    }
  }
}