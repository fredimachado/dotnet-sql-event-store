using System.Collections.Concurrent;
using System.Reflection;
using WareHouseApi.Domain;

namespace WareHouseApi.Infrastructure;

public static class EventTypeResolver
{
    private readonly static ConcurrentDictionary<string, Type> eventTypes = new ConcurrentDictionary<string, Type>();

    public static void RegisterEvents<T>()
    {
        RegisterEvents(typeof(T).Assembly);
    }

    public static void RegisterEvents(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Event).IsAssignableFrom(t))
            .ToList();

        foreach (var type in types)
        {
            if (!eventTypes.TryAdd(type.Name, type))
            {
                throw new InvalidOperationException($"Event type '{type.Name}' is already registered.");
            }
        }
    }

    public static Type GetEventType(string eventName)
    {
        if (eventTypes.IsEmpty)
        {
            throw new InvalidOperationException("Resolver needs to be initialized.");
        }

        return eventTypes[eventName];
    }
}