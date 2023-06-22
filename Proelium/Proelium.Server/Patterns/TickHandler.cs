using Proelium.Server.General;

namespace Proelium.Shared.Patterns;

public abstract class TickHandler
{
    protected Context Ctx { get; init; }

    public TickHandler(Context ctx)
    {
        Ctx = ctx;
    }

    public abstract void Tick();
}