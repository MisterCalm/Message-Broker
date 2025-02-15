using System.Reflection;
using SharedLibrary;
namespace Consumer;
public static class ConsumerLoader
{
    public static List<(object Instance, MethodInfo Method)> LoadConsumers()
{
    var consumers = new List<(object Instance, MethodInfo Method)>();
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..", "..", "..", "AddOns", "ConsumerDll");
    var dllFiles = Directory.GetFiles(path, "*.dll");

    foreach (var dll in dllFiles)
    {
        var assembly = Assembly.LoadFile(Path.GetFullPath(dll));
        Console.WriteLine($"Loaded Assembly: {assembly.FullName}");
        var types = assembly.GetTypes();
        Console.WriteLine($"Types found in assembly: {string.Join(", ", types.Select(t => t.FullName))}");

        foreach (var type in types)
{
    Console.WriteLine($"Inspecting Type: {type.FullName}");
    var interfaces = type.GetInterfaces();
    foreach (var i in interfaces)
    {
        if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
        {
            var instance = Activator.CreateInstance(type);
            var methods = type.GetMethods();

            Console.WriteLine($"Methods in {type.FullName}:");
            foreach (var method in methods)
            {
                Console.WriteLine($"Method: {method.Name}, Parameters: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType))}");
            }

            foreach (var method in methods)
            {
                Console.WriteLine($"Inspecting Method: {method.Name}");

                // Check for SubscribeIdAttribute directly
                var subscribeAttr = method.GetCustomAttribute<SubscribeIdAttribute>();
                if (subscribeAttr != null)
                {
                    Console.WriteLine($"Found SubscribeIdAttribute: ProducerId = {subscribeAttr.producerId}");
                    consumers.Add((instance, method));
                }
                else
                {
                    Console.WriteLine("No SubscribeIdAttribute found.");
                }
            }
        }
    }
}

    }

    Console.WriteLine($"Loaded {consumers.Count} consumers.");
    return consumers;
}

}
