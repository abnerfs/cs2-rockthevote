using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace cs2_rockthevote
{
    public class DependencyManager<TPlugin, TConfig>
    {
        private List<IPluginDependency<TPlugin, TConfig>> Dependencies { get; set; } = new();

        private List<Type> TypesToAdd { get; set; } = new();

        Type dependencyType = typeof(IPluginDependency<TPlugin, TConfig>);

        public void LoadDependencies(Assembly assembly)
        {
            
            var typesToAdd = assembly.GetTypes()
                .Where(x => x.IsClass)
                .Where(dependencyType.IsAssignableFrom);

            TypesToAdd.AddRange(typesToAdd);            
        }

        public void AddIt(IServiceCollection collection)
        {
            foreach (var type in TypesToAdd)
            {
                collection.AddSingleton(type);
                Console.WriteLine(type.Name);
            }

            collection.AddSingleton(p =>
            {
                Dependencies = TypesToAdd
                    .Where(x => dependencyType.IsAssignableFrom(x))
                    .Select(type => (IPluginDependency<TPlugin, TConfig>) p.GetService(type)!)
                    .ToList();

                return this;
            });
        }

        public void OnMapStart(string mapName)
        {
            foreach(var service in Dependencies)
            {
                Console.WriteLine($"Calling OnMapStart on {service.GetType().Name}");
                service.OnMapStart(mapName);
            }
        }

        public void OnPluginLoad(TPlugin plugin)
        {
            foreach (var service in Dependencies)
            {
                Console.WriteLine($"Calling OnPluginLoad on {service.GetType().Name}");
                service.OnLoad(plugin);
            }
        }

        public void OnConfigParsed(TConfig config)
        {
            foreach (var service in Dependencies)
            {
                Console.WriteLine($"Calling OnConfigParsed on {service.GetType().Name}");
                service.OnConfigParsed(config);
            }
        }
    }
}
