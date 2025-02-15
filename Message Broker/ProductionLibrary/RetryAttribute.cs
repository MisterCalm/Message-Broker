namespace ProductionLibrary;

[AttributeUsage(AttributeTargets.Method)]
public class RetryAttribute : Attribute
{
    public int RetryCount { get; }
    public RetryAttribute(int retryCount)
    {
        RetryCount = retryCount;
    }
}