using System.Collections.Generic;

namespace NeoAnimLib
{
    internal static class ListPool<T>
    {
        private static readonly Queue<List<T>> pool = new Queue<List<T>>();

        public static Scope Borrow(out List<T> list)
        {
            lock (pool)
            {
                if (!pool.TryDequeue(out list))
                    list = new List<T>();
            }

            return new Scope(list);
        }

        public readonly ref struct Scope
        {
            private readonly List<T> list;

            public Scope(List<T> list)
            {
                this.list = list;
            }

            public void Dispose()
            {
                list.Clear();
                lock (pool)
                {
                    pool.Enqueue(list);
                }
            }
        }
    }
}
