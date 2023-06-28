using System.Diagnostics;
using System.Numerics;

namespace Proelium.Ecs;

public class Test
{
    public class Transform : Component
    {
        public Vector2 position;
    }

    public class Body : Component
    {
        public Vector2 velocity;
    }

    public class PhysicsQuery : Query
    {
        public record struct Aspect(Transform Transform, Body Body);

        public readonly Dictionary<ulong, Aspect> aspects = new();

        public override IEnumerable<Type> GetComponentTypes()
        {
            yield return typeof(Transform);
            yield return typeof(Body);
        }

        public override void OnAddComponent(Component component, Entity entity)
        {
            if (entity.TryGet<Transform>(out var transform) && entity.TryGet<Body>(out var body))
            {
                aspects[entity.entityId] = new(transform, body);
            }
        }

        public override void OnRemoveComponent(Component component, Entity entity)
        {
            if (entity.Contains<Transform>() && entity.Contains<Body>())
            {
                return;
            }

            aspects.Remove(entity.entityId);
        }
    }

    public static void Run()
    {
        EntityManager entityManager = new();

        PhysicsQuery physicsQuery = entityManager.GetQuery<PhysicsQuery>();

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1_000_000; i++)
        {
            var entity = entityManager.CreateEntity();
            entityManager.AddComponent<Transform>(entity.entityId);
            entityManager.AddComponent<Body>(entity.entityId);
        }
        stopwatch.Stop();
        Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds}ms");

        stopwatch.Restart();
        for (ulong i = 0; i < 500_000; i++)
        {
            entityManager.RemoveComponent<Transform>(i);
            entityManager.RemoveComponent<Body>(i);
        }
        stopwatch.Stop();
        Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds}ms");

        stopwatch.Restart();
        foreach (var aspect in physicsQuery.aspects.Values)
        {
            aspect.Transform.position += aspect.Body.velocity;
        }
        stopwatch.Stop();
        Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds}ms");

        Console.ReadKey();
    }
}