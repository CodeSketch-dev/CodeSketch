using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSketch.Optimize
{
    public static class OptCollisionLookup
    {
        // Collider → (Type → List<Owner>)
        static readonly Dictionary<Collider, Dictionary<Type, List<object>>> _map = new();

        // Reusable iteration buffer – avoids allocation and guards against collection-modified errors
        static readonly List<object> _iterBuffer = new(8);

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
                    typeMap = new Dictionary<Type, List<object>>(4);
                    _map[col] = typeMap;
                }

                if (!typeMap.TryGetValue(type, out var list))
                {
                    list = new List<object>(2);
                    typeMap[type] = list;
                }

                if (!list.Contains(owner)) // tránh double register
                    list.Add(owner);
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
                if (!typeMap.TryGetValue(type, out var list)) continue;

                list.Remove(owner);

                if (list.Count == 0)
                    typeMap.Remove(type);

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

            if (!typeMap.TryGetValue(typeof(T), out var list))
                return;

            // Copy to buffer before iterating so that any Register/Unregister
            // triggered inside action() cannot cause "collection was modified" errors.
            _iterBuffer.Clear();
            _iterBuffer.AddRange(list);

            for (int i = 0; i < _iterBuffer.Count; i++)
            {
                if (_iterBuffer[i] is T match)
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

            if (!typeMap.TryGetValue(typeof(T), out var list)) return false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is T match)
                {
                    owner = match;
                    return true;
                }
            }

            return false;
        }

        // =========================================================

        public static void Clear()
        {
            _map.Clear();
        }
    }
}
