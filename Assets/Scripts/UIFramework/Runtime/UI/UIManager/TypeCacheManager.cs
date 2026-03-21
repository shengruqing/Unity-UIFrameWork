using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 类型查找缓存管理器。
    /// 提供高效的类型查找和缓存功能，避免重复的反射操作。
    /// </summary>
    public static class TypeCacheManager
    {
        private static readonly ConcurrentDictionary<string, Type> _typeCache = new();
        private static readonly ConcurrentDictionary<string, Assembly> _assemblyCache = new();
        private static readonly HashSet<string> _searchedAssemblies = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// 查找View类型（带缓存）。
        /// </summary>
        /// <param name="viewName">View名称。</param>
        /// <returns>找到的类型，未找到返回null。</returns>
        public static Type FindViewType(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
                return null;

            // 先从缓存查找
            if (_typeCache.TryGetValue(viewName, out var cachedType))
            {
                return cachedType;
            }

            lock (_lock)
            {
                // 双重检查
                if (_typeCache.TryGetValue(viewName, out cachedType))
                {
                    return cachedType;
                }

                // 执行查找
                var foundType = SearchForViewType(viewName);
                
                if (foundType != null)
                {
                    _typeCache.TryAdd(viewName, foundType);
                }
                else
                {
                    Logger.Warning($"[TypeCache] 未找到类型: {viewName}");
                }

                return foundType;
            }
        }

        /// <summary>
        /// 实际的类型查找逻辑。
        /// </summary>
        /// <param name="viewName">View名称。</param>
        /// <returns>找到的类型。</returns>
        private static Type SearchForViewType(string viewName)
        {
            // 获取当前程序集
            var currentAssembly = typeof(TypeCacheManager).Assembly;
            
            // 1. 在当前程序集中查找
            var type = currentAssembly.GetType(viewName);
            if (IsValidViewType(type))
            {
                return type;
            }

            // 2. 尝试带命名空间的查找
            string fullName = "GameLogic." + viewName;
            type = currentAssembly.GetType(fullName);
            if (IsValidViewType(type))
            {
                return type;
            }

            // 3. 在已缓存的程序集中查找
            foreach (var assemblyEntry in _assemblyCache)
            {
                type = assemblyEntry.Value.GetType(viewName);
                if (IsValidViewType(type))
                {
                    return type;
                }

                type = assemblyEntry.Value.GetType(fullName);
                if (IsValidViewType(type))
                {
                    return type;
                }
            }

            // 4. 在所有程序集中查找（只执行一次）
            if (!_searchedAssemblies.Contains("ALL_ASSEMBLIES"))
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    // 跳过系统程序集以提高性能
                    if (IsSystemAssembly(assembly))
                        continue;

                    // 缓存程序集
                    if (!_assemblyCache.ContainsKey(assembly.GetName().Name))
                    {
                        _assemblyCache.TryAdd(assembly.GetName().Name, assembly);
                    }

                    type = assembly.GetType(viewName);
                    if (IsValidViewType(type))
                    {
                        return type;
                    }

                    type = assembly.GetType(fullName);
                    if (IsValidViewType(type))
                    {
                        return type;
                    }
                }
                _searchedAssemblies.Add("ALL_ASSEMBLIES");
            }

            return null;
        }

        /// <summary>
        /// 验证类型是否为有效的View类型。
        /// </summary>
        /// <param name="type">要验证的类型。</param>
        /// <returns>是否为有效View类型。</returns>
        private static bool IsValidViewType(Type type)
        {
            return type != null && 
                   typeof(UIView).IsAssignableFrom(type) && 
                   !type.IsAbstract &&
                   !type.IsInterface;
        }

        /// <summary>
        /// 判断是否为系统程序集。
        /// </summary>
        /// <param name="assembly">程序集。</param>
        /// <returns>是否为系统程序集。</returns>
        private static bool IsSystemAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.StartsWith("System") || 
                   name.StartsWith("Microsoft") || 
                   name.StartsWith("Unity") ||
                   name.StartsWith("mscorlib") ||
                   name.StartsWith("netstandard");
        }

        /// <summary>
        /// 预加载常用类型到缓存。
        /// </summary>
        /// <param name="typeNames">类型名称数组。</param>
        public static void PreloadTypes(string[] typeNames)
        {
            if (typeNames == null) return;
            
            foreach (var typeName in typeNames)
            {
                if (!string.IsNullOrEmpty(typeName))
                {
                    FindViewType(typeName);
                }
            }
        }

        /// <summary>
        /// 清空类型缓存。
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _typeCache.Clear();
                _assemblyCache.Clear();
                _searchedAssemblies.Clear();
                Logger.Log("[TypeCache] 缓存已清空");
            }
        }

        /// <summary>
        /// 获取缓存统计信息。
        /// </summary>
        /// <returns>缓存统计信息。</returns>
        public static string GetCacheStats()
        {
            return $"类型缓存: {_typeCache.Count} 个, 程序集缓存: {_assemblyCache.Count} 个";
        }
    }
}