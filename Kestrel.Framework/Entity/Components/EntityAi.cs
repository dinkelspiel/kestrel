using System.Numerics;
using LiteNetLib.Utils;

namespace Kestrel.Framework.Entity.Components;

public abstract record EntityState
{
    public float? StateTime = null;
};

public record EntityIdle : EntityState
{
    public EntityIdle()
    {
        StateTime = 5f;
    }
};

public record EntityWalking : EntityState
{
    public Vector3 TargetLocation;

    public EntityWalking(Vector3 location)
    {
        TargetLocation = location;
        StateTime = 5f;
    }
};

public record struct EntityAi
{
    public EntityState State;
    public DateTime LastStateChange;

    public EntityAi(EntityState State)
    {
        this.State = State;
        LastStateChange = DateTime.Now;
    }
};
