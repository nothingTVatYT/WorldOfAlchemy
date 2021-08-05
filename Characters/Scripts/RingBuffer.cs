using System.Collections;
using System.Collections.Generic;

public class RingBuffer<T> : IEnumerable<T>
{
	protected int capacity;
	protected int index;
	protected int filled;
	T[] data;

	public RingBuffer (int capacity)
	{
		this.capacity = capacity;
		data = new T[capacity];
		index = 0;
		filled = 0;
	}

    public int Count { get { return filled; }}

	public T[] ToArray ()
	{
		T[] arr = new T[filled];
		int first = FirstIndex();
		for (int i = 0; i < filled; i++) {
			arr [i] = data [wrappedIndex (first + i)];
		}
		return arr;
	}

	int FirstIndex() {
		return (filled < capacity) ? 0 : wrappedIndex (index + 1);
	}

    public void Add(T item)
    {
        if (index >= capacity)
			index = 0;
		data [index++] = item;
		if (filled < index)
			filled = index;
    }

    public void Clear()
    {
        index = 0;
		filled = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        int position = 0;
		int first = FirstIndex();
    	while(position < filled) {
			yield return data[wrappedIndex(first + position++)];
		}
    }

    int wrappedIndex (int i)
	{
		if (i >= filled || i >= capacity)
			return i % filled;
		return i;
	}

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}

