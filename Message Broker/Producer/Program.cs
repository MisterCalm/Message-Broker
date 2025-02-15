using System.Reflection;
using System.Reflection;

namespace ProducerNameSpace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Get the path to the DLLs
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..", "..", "..", "AddOns", "ProducerDll");
            var dllFiles = Directory.GetFiles(pluginsPath, "*.dll");

            // Load each DLL and find the types implementing IProducer<>
            foreach (var dll in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dll);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // Find the first interface that is IProducer<>
                    var producerInterface = type.GetInterface("IProducer`1");
                    if (producerInterface != null)
                    {
                        // Get the type argument (the T in IProducer<T>)
                        var messageType = producerInterface.GetGenericArguments()[0];

                        // Make Producer<T> with the correct type and invoke the constructor
                        var producerType = typeof(Producer<>).MakeGenericType(messageType);
                        var producerInstance = Activator.CreateInstance(producerType);

                        if (producerInstance != null)
                        {
                            // Load producers from the DLLs
                            producerType.GetMethod("LoadProducers")!.Invoke(producerInstance, null);

                            // Start producing messages
                            producerType.GetMethod("ProduceMessages")!.Invoke(producerInstance, null);

                            Console.WriteLine($"Started producer for type: {messageType.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create instance for producer type: {producerType.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Type {type.FullName} does not implement IProducer<T>");
                    }
                }
            }

            Console.WriteLine("All producers are running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
