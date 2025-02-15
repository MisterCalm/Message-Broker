namespace ProductionLibrary;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitAttribute : Attribute
{
    public int ThreadCount { get; }
    public RateLimitAttribute(int threadCount)
    {
        ThreadCount = threadCount;
    }
}
