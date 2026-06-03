using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSketch.Optimize
{
    /// <summary>
    /// Instance wrapper của OptCollisionLookup kế thừa MonoCachedFast.
    /// Dùng khi cần reference component hoặc gọi qua dependency injection.
    /// Hot path vẫn delegate về static TypedMap — không overhead thêm.
    /// </summary>
    public class OptCollisionLookupFast : CodeSketch.MonoCachedFast
    {
        // =========================================================
        // REGISTER
        // =========================================================

        public void Register<T>(T owner, Collider[] colliders) where T : class
            => OptCollisionLookup.Register<T>(owner, colliders);

        public void Unregister<T>(T owner, Collider[] colliders) where T : class
            => OptCollisionLookup.Unregister<T>(owner, colliders);

        // =========================================================
        // QUERY
        // =========================================================

        /// <summary>
        /// Trả về owner đầu tiên type T registered cho collider này.
        /// Overload mới — khác signature với GetCached(bool, bool) kế thừa từ MonoCachedFast.
        /// </summary>
        public T GetCached<T>(Collider collider) where T : class
        {
            OptCollisionLookup.TryGetFirst<T>(collider, out var result);
            return result;
        }

        public bool TryGetFirst<T>(Collider collider, out T owner) where T : class
            => OptCollisionLookup.TryGetFirst<T>(collider, out owner);

        public void ForEach<T>(Collider collider, Action<T> action) where T : class
            => OptCollisionLookup.ForEach<T>(collider, action);

        public void ForEach<T, TAction>(Collider collider, ref TAction action)
            where T : class
            where TAction : struct, IOptCollisionAction<T>
            => OptCollisionLookup.ForEach<T, TAction>(collider, ref action);

        public void GetSnapshot<T>(Collider collider, List<T> result) where T : class
            => OptCollisionLookup.GetSnapshot<T>(collider, result);

        // =========================================================

        public void Clear() => OptCollisionLookup.Clear();
    }
}
