using Proelium.Server.General;

namespace Proelium.Shared.Patterns;

public abstract class StartHandler
{   
    protected Context Ctx { get; init; }

    public StartHandler(Context ctx)
    {
        Ctx = ctx;
    }

    public abstract void Start();
}