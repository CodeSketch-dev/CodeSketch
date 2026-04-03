using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSketch.Optimize
{
    public static class OptCollisionLookup
    {
        // Collider → (Type → Owner array snapshot)
        // Use immutable snapshot arrays for lock-free fast reads (no per-query allocation).
        static readonly Dictionary<Collider, Dictionary<Type, object[]>> _map = new();

        // =========================================================
        // REGISTER
        // =========================================================

        public static void Register(Type type, object owner, Collider[] colliders)
        {
            if (type == null || owner == null || colliders == null)
                return;
            foreach (var col in colliders)
            {
                if (!col) continue;

                if (!_map.TryGetValue(col, out var typeMap))
                {
                    typeMap = new Dictionary<Type, object[]>(4);
                    _map[col] = typeMap;
                }

                if (!typeMap.TryGetValue(type, out var arr))
                {
                    // create single-entry array
                    typeMap[type] = new object[] { owner };
                    continue;
                }

                // check existing
                bool found = false;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (ReferenceEquals(arr[i], owner)) { found = true; break; }
                }

                if (found) continue;

                // append by creating new snapshot array
                var newArr = new object[arr.Length + 1];
                Array.Copy(arr, newArr, arr.Length);
                newArr[arr.Length] = owner;
                typeMap[type] = newArr;
            }
        }

        public static void Unregister(Type type, object owner, Collider[] colliders)
        {
            if (type == null || owner == null || colliders == null)
                return;
            foreach (var col in colliders)
            {
                if (!col) continue;
                if (!_map.TryGetValue(col, out var typeMap)) continue;
                if (!typeMap.TryGetValue(type, out var arr)) continue;

                int idx = -1;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (ReferenceEquals(arr[i], owner)) { idx = i; break; }
                }

                if (idx < 0) continue;

                if (arr.Length == 1)
                {
                    typeMap.Remove(type);
                }
                else
                {
                    var newArr = new object[arr.Length - 1];
                    if (idx > 0) Array.Copy(arr, 0, newArr, 0, idx);
                    if (idx < arr.Length - 1) Array.Copy(arr, idx + 1, newArr, idx, arr.Length - idx - 1);
                    typeMap[type] = newArr;
                }

                if (typeMap.Count == 0)
                    _map.Remove(col);
            }
        }

        // =========================================================
        // QUERY – HOT PATH
        // =========================================================

        public static void ForEach<T>(Collider collider, Action<T> action)
            where T : class
        {
            if (!collider || action == null)
                return;

            if (!_map.TryGetValue(collider, out var typeMap))
                return;

            if (!typeMap.TryGetValue(typeof(T), out var arr))
                return;

            // arr is an immutable snapshot array — safe to iterate without copying
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] is T match)
                    action(match);
            }
        }

        /// <summary>
        /// Try get first registered owner of type T for the given collider.
        /// Fast path when only a single owner is needed (avoids copying iteration buffer).
        /// </summary>
        public static bool TryGetFirst<T>(Collider collider, out T owner) where T : class
        {
            owner = null;
            if (!collider) return false;

            if (!_map.TryGetValue(collider, out var typeMap)) return false;

            if (!typeMap.TryGetValue(typeof(T), out var arr)) return false;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] is T match)
                {
                    owner = match;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Lấy snapshot các owner của type T đã đăng ký cho collider vào List tái sử dụng.
        /// - Không tạo mảng nội tại (không allocate object[] mới ngoài arr),
        /// - Copy các reference vào `result` do caller cung cấp để tránh allocations bên trong.
        /// Sử dụng khi cần duyệt nhiều owner nhưng muốn kiểm soát bộ nhớ (pass reuse List).
        /// </summary>
        public static void GetSnapshot<T>(Collider collider, System.Collections.Generic.List<T> result) where T : class
        {
            if (result == null)
                return;

            result.Clear();

            if (!collider) return;

            if (!_map.TryGetValue(collider, out var typeMap)) return;

            if (!typeMap.TryGetValue(typeof(T), out var arr)) return;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] is T match)
                    result.Add(match);
            }
        }

        // =========================================================

        public static void Clear()
        {
            _map.Clear();
        }
    }
}
