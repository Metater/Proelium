namespace Proelium.Server.Ecs;

public class EntityManager
{
    private readonly Dictionary<Type, object> components = new();
    private readonly SortedList<ulong, Random> test = new(); 
}