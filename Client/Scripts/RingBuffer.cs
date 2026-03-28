using System.Collections;
using System.Collections.Generic;

namespace Soteo.Client
{
    public sealed class RingBuffer<T> : IEnumerable<T>
    {
        private T[] _data;
        private int _size;
        
        public RingBuffer(int size)
        {
            _data = new T[size];
            _size = size;
        }
        
        public T this[long i]
        {
            get => _data[i % _size];
            set => _data[i % _size] = value;
        }
        
        public void Fill(T value)
        {
            for (int i = 0; i < _size; i++) _data[i] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}