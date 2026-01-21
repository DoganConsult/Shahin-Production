namespace GrcMvc.Application.Policy;

public interface IDotPathResolver
{
    object? Resolve(object obj, string path);
    bool Exists(object obj, string path);
    void Set(object obj, string path, object? value);
    void Remove(object obj, string path);
}
