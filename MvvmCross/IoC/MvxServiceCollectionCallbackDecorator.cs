using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MvvmCross.IoC
{
    public class MvxServiceCollectionCallbackDecorator : IServiceCollection
    {
        private readonly IServiceCollection _collection;
        public IDictionary<Type, List<Action>> Waiters { get; } = new Dictionary<Type, List<Action>>();

        public MvxServiceCollectionCallbackDecorator(IServiceCollection collection)
        {
            _collection = collection;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            _collection.Add(item);
            NotifyWaiters(item);
        }

        public void Clear()
        {
            Waiters.Clear();
            _collection.Clear();
        }

        public bool Contains(ServiceDescriptor item) => _collection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public bool Remove(ServiceDescriptor item) => _collection.Remove(item);

        public int Count => _collection.Count;
        public bool IsReadOnly => _collection.IsReadOnly;

        public int IndexOf(ServiceDescriptor item) => _collection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item)
        {
            _collection.Insert(index, item);
        }

        public void RemoveAt(int index) => _collection.RemoveAt(index);

        public ServiceDescriptor this[int index]
        {
            get => _collection[index];
            set => _collection[index] = value;
        }
        
        private void NotifyWaiters(ServiceDescriptor item)
        {
            if (Waiters.TryGetValue(item.ServiceType, out var actions))
            {
                Waiters.Remove(item.ServiceType);
            }

            if (actions == null) return;
            
            foreach (var action in actions)
            {
                action();
            }
        }
    }
}
