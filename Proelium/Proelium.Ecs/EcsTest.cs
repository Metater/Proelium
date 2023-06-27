using System.Numerics;

namespace Proelium.Ecs;

public class EcsTest
{
    public class Transform : Component
    {
        public Vector2 position;
    }

    public class Body : Component
    {
        public Vector2 velocity;
    }

    public record struct PhysicsBodyAspect(Transform Transform, Body Body);

    public class PhysicsBodyQuery : Query
    {
        private readonly Dictionary<ulong, PhysicsBodyAspect> bodies = new();

        public override IEnumerable<Type> GetComponentTypes()
        {
            yield return typeof(Transform);
            yield return typeof(Body);
        }

        public override void OnAddComponent(Component component, Entity entity)
        {
            if (entity.TryGet<Transform>(out var transform) && entity.TryGet<Body>(out var body))
            {
                bodies[entity.entityId] = new(transform, body);
            }
        }

        public override void OnRemoveComponent(Component component, Entity entity)
        {
            if (!entity.Contains<Transform>() || !entity.Contains<Body>())
            {
                bodies.Remove(entity.entityId);
            }
        }
    }

    public static void Run()
    {

    }
}