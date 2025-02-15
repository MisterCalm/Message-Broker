namespace SharedLibrary;

[AttributeUsage(AttributeTargets.Method)]
public class SubscribeIdAttribute : Attribute
{
    public int producerId{get;}
    public SubscribeIdAttribute(int id)
    {
        producerId = id;
    }

}