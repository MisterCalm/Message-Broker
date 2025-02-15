namespace Consumer;
public static class RateLimiter
{
    private static readonly Dictionary<int, DateTime> LastConsumed = new();

    public static bool CanConsume(int rate)
    {
        var interval = TimeSpan.FromSeconds(1.0 / rate);

        if (LastConsumed.TryGetValue(rate, out var lastTime))
        {
            if (DateTime.Now - lastTime < interval)
            {
                return false;
            }
        }

        LastConsumed[rate] = DateTime.Now;
        return true;
    }
}
