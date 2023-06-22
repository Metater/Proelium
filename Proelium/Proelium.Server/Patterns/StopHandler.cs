using Proelium.Server.General;

namespace Proelium.Shared.Patterns;

public abstract class StopHandler
{
    protected Context Ctx { get; init; }

    public StopHandler(Context ctx)
    {
        Ctx = ctx;
    }

    public abstract void Stop();
}