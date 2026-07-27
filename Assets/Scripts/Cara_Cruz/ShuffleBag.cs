using System.Collections.Generic;
using UnityEngine;

public class ShuffleBag<T>
{
    private List<T> _items;
    private List<T> _originalItems;
    private int _currentIndex;

    public ShuffleBag(IEnumerable<T> items)
    {
        _originalItems = new List<T>(items);
        _items = new List<T>(_originalItems);
        Shuffle();
        _currentIndex = 0;
    }

    public T Next()
    {
        if (_currentIndex >= _items.Count)
        {
            Reset();
        }

        T item = _items[_currentIndex];
        _currentIndex++;
        return item;
    }

    public void Reset()
    {
        _items = new List<T>(_originalItems);
        Shuffle();
        _currentIndex = 0;
    }

    public int Remaining => _items.Count - _currentIndex;

    private void Shuffle()
    {
        for (int i = _items.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = _items[i];
            _items[i] = _items[j];
            _items[j] = temp;
        }
    }
}
