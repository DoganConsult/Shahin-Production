using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Resolves dot-path expressions into object properties with caching for performance
/// </summary>
public class DotPathResolver : IDotPathResolver
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DotPathResolver> _logger;

    public DotPathResolver(IMemoryCache cache, ILogger<DotPathResolver> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public object? Resolve(object obj, string path)
    {
        if (string.IsNullOrEmpty(path))
            return obj;

        var cacheKey = $"{obj.GetType().FullName}:{path}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            var parts = path.Split('.');
            object? current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                // Handle array index notation like "Items[0]"
                var arrayMatch = System.Text.RegularExpressions.Regex.Match(part, @"^(\w+)\[(\d+)\]$");
                if (arrayMatch.Success)
                {
                    var propertyName = arrayMatch.Groups[1].Value;
                    var index = int.Parse(arrayMatch.Groups[2].Value);

                    // First resolve the property
                    current = ResolvePropertyOrField(current, propertyName);
                    if (current == null)
                        return null;

                    // Then access the array index
                    if (current is System.Collections.IList list)
                    {
                        current = index < list.Count ? list[index] : null;
                    }
                    else if (current is Array array)
                    {
                        current = index < array.Length ? array.GetValue(index) : null;
                    }
                    else
                    {
                        return null;
                    }
                    continue;
                }

                // Handle dictionary
                if (current is System.Collections.IDictionary dict)
                {
                    current = dict.Contains(part) ? dict[part] : null;
                    continue;
                }

                // Handle PolicyResourceWrapper specifically
                if (current is PolicyResourceWrapper wrapper)
                {
                    current = part.ToLower() switch
                    {
                        "id" => wrapper.Id,
                        "title" => wrapper.Title,
                        "type" => wrapper.Type,
                        "metadata" => wrapper.Metadata,
                        "resource" => wrapper.Resource,
                        _ => null
                    };
                    if (current != null) continue;
                }

                // Handle PolicyResourceMetadata
                if (current is PolicyResourceMetadata metadata)
                {
                    current = part.ToLower() switch
                    {
                        "labels" => metadata.Labels,
                        "additional" => metadata.Additional,
                        _ => null
                    };
                    if (current != null) continue;
                }

                // Handle JSON element
                if (current is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty(part, out var prop))
                    {
                        current = prop;
                        continue;
                    }
                    return null;
                }

                // Handle object properties via reflection
                var type = current.GetType();
                var propInfo = type.GetProperty(part, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.IgnoreCase);

                if (propInfo != null)
                {
                    current = propInfo.GetValue(current);
                }
                else
                {
                    // Try field access for anonymous objects or compiler-generated types
                    var fieldInfo = type.GetField(part,
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.IgnoreCase);
                    
                    if (fieldInfo != null)
                    {
                        current = fieldInfo.GetValue(current);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            _cache.Set(cacheKey, current, TimeSpan.FromMinutes(5));
            return current;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving path {Path} on {Type}", path, obj.GetType().Name);
            return null;
        }
    }

    public bool Exists(object obj, string path)
    {
        return Resolve(obj, path) != null;
    }

    public void Set(object obj, string path, object? value)
    {
        var parts = path.Split('.');
        object? current = obj;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            current = Resolve(current, parts[i]);
            if (current == null)
                throw new InvalidOperationException($"Path segment {parts[i]} does not exist");
        }

        var finalPart = parts[^1];
        var type = current!.GetType();
        var propInfo = type.GetProperty(finalPart,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        if (propInfo != null && propInfo.CanWrite)
        {
            propInfo.SetValue(current, value);
        }
        else
        {
            throw new InvalidOperationException($"Cannot set property {finalPart} on {type.Name}");
        }
    }

    public void Remove(object obj, string path)
    {
        Set(obj, path, null);
    }

    private object? ResolvePropertyOrField(object current, string name)
    {
        var type = current.GetType();
        var propInfo = type.GetProperty(name,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        if (propInfo != null)
        {
            return propInfo.GetValue(current);
        }

        var fieldInfo = type.GetField(name,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        return fieldInfo?.GetValue(current);
    }
}
