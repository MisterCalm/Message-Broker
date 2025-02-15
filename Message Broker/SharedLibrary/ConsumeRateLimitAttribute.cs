namespace SharedLibrary;

[AttributeUsage(AttributeTargets.Method)]
public class ConsumeRateLimitAttribute : Attribute
{
    public int rate{get;}
    public ConsumeRateLimitAttribute(int Rate)
    {
        rate = Rate;
    }

}