using TubeOrchestrator.Core.Entities;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// Context object containing all information needed by agents during job processing
/// </summary>
public class JobContext
{
    public Job Job { get; set; } = null!;
    public Channel Channel { get; set; } = null!;
    
    // Data accumulated during job processing
    public Dictionary<string, object> Data { get; set; } = new();
    
    // Helper methods to store/retrieve typed data
    public void Set<T>(string key, T value) where T : notnull
    {
        Data[key] = value;
    }
    
    public T? Get<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
    
    public bool TryGet<T>(string key, out T? value)
    {
        if (Data.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default;
        return false;
    }
}
