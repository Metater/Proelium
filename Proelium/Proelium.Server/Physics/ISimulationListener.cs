namespace Proelium.Server.Physics;

public interface ISimulationListener
{
    public void OnRepulsion(Entity repulsee, Entity repulsor);
    public void OnTrigger(Entity triggeree, Entity triggerer);
    public void OnStop(Entity stopee, Entity stopper, Direction direction);
    public void OnBoundsStop(Entity entity, Direction direction);
}