using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoiledCat.Utils.Collections
{
	public class HashTable<Key, T> : ISet<T> where T : class, IKeyedObject<Key>
	{
		Dictionary<Key, T> items = new Dictionary<Key, T>();

		public T this[Key key] {
			get { return items[key];  }
		}

		public T this[T item] {
			get {
				return items[item.Key]; }
		}

		// Summary:
		//     Adds an element to the current set and returns the old value, if there was one
		//
		// Parameters:
		//   item:
		//     The element to add to the set.
		//
		// Returns:
		//     the old element with the same key if there was one
		public T Add(T item)
		{
			var key = item.Key;
			if (items.ContainsKey(key))
				return Replace(item);
			items.Add(key, item);
			return default(T);
		}

		public T Replace(T item)
		{
			var old = Get(item);
			var key = item.Key;
			items[key] = item;
			return old;
		}

		public bool Remove(T item)
		{
			if (!Contains(item))
				return false;
			var key = item.Key;
			items.Remove(key);
			return true;
		}

		public T Remove(Key key)
		{
			if (!Contains(key))
				return null;
			var ret = items[key];
			items.Remove(key);
			return ret;
		}

		public T Get(Key key)
		{
			if (!items.ContainsKey(key))
				throw new ArgumentException("Invalid key", "key");
			return items[key];
		}

		public T Get(T item)
		{
			var key = item.Key;
			if (!items.ContainsKey(key))
				throw new ArgumentException("Invalid key", "key");
			return items[key];
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			foreach (var item in other) {
				items.Remove(item.Key);
			}
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			var old = items.ToArray();
			foreach (var item in old) {
				if (!other.Contains(item.Value))
					items.Remove(item.Key);
			}
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		public void Clear()
		{
			items.Clear();
		}

		public bool Contains(T item)
		{
			return items.ContainsKey(item.Key);
		}

		public bool Contains(Key item)
		{
			return items.ContainsKey(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			array = items.Values.ToArray();
		}

		public int Count
		{
			get { return items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return items.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return items.Values.GetEnumerator();
		}

		#region DonCare
		bool ISet<T>.Add(T item)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.Overlaps(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		bool ISet<T>.SetEquals(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}

		void ISet<T>.UnionWith(IEnumerable<T> other)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
