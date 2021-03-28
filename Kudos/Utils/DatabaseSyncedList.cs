#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kudos.DatabaseModels;
using Microsoft.EntityFrameworkCore;
#endregion

namespace Kudos.Utils {
	public class DatabaseSyncedList {
		private static Dictionary<Type, IEnumerable> Instances { get; } = new();

		public static DatabaseSyncedList<T> Instance<T>()
			where T : class {
			if (Instances.ContainsKey(typeof (T))) {
				return (DatabaseSyncedList<T>)Instances[typeof (T)];
			}
			DatabaseSyncedList<T> instance = new();
			Instances.Add(typeof (T), instance);
			return instance;
		}
	}

	public class DatabaseSyncedList<T> : IList<T>
		where T : class {
		public int Count => Set.Count();
		protected KudosDataContext DbContext => new();

		public bool IsReadOnly => false;

		protected DbSet<T> Set => DbContext.Set<T>();

		public T this[int index] {
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		/*
		 * Do not use this constructor, use Instance of not generic class
		 */
		internal DatabaseSyncedList() { }

		protected Tuple<KudosDataContext, DbSet<T>> FullContext() {
			KudosDataContext context = DbContext;
			return new Tuple<KudosDataContext, DbSet<T>>(context, context.Set<T>());
		}

		// ReSharper disable once UnusedMember.Global
		public void Update(T item) {
			(KudosDataContext context, DbSet<T> set) = FullContext();
			set.Update(item ?? throw new ArgumentNullException(nameof (item)));
			context.SaveChanges(true);
		}

		public void Add(T item) {
			(KudosDataContext context, DbSet<T> set) = FullContext();
			set.Add(item ?? throw new ArgumentNullException(nameof (item)));
			context.SaveChanges(true);
		}

		public void Clear() {
			(KudosDataContext context, DbSet<T> set) = FullContext();
			set.RemoveRange(Set);
			context.SaveChanges(true);
		}

		public bool Contains(T item) => Set.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) {
			Set.ToList().CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {
			(KudosDataContext context, DbSet<T> set) = FullContext();
			set.Remove(item ?? throw new ArgumentNullException(nameof (item)));
			context.SaveChanges();
			return true;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<T> GetEnumerator() => Set.ToEnumerable().GetEnumerator();
		public int IndexOf(T item) => throw new NotSupportedException();

		public void Insert(int index, T item) {
			throw new NotSupportedException();
		}

		public void RemoveAt(int index) {
			throw new NotSupportedException();
		}
	}
}
