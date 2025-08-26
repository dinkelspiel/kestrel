namespace Kestrel.Framework.Networking;

using System;
using System.Linq;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions; // for Entity extension helpers
using Arch.Core.Utils;      // for Signature
using Kestrel.Framework.Entity;

public static class NetworkSnapshot
{
    static readonly Type[] NetworkableTypes = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(a =>
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        })
        .Where(t => typeof(INetworkableComponent).IsAssignableFrom(t)
                    && t.IsValueType && !t.IsAbstract)
        .ToArray();

    static ComponentType? TryGetComponentType(Type t)
    {
        try { return Arch.Core.ComponentRegistry.TypeToComponentType[t]; }
        catch { return null; } // not registered yet, ignore
    }

    static readonly Signature AnyNetworkableSig = new Signature(NetworkableTypes);

    public static Dictionary<int, INetworkableComponent[]> Build(World world)
    {
        var result = new Dictionary<int, INetworkableComponent[]>();
        var q = new QueryDescription(any: AnyNetworkableSig);

        world.Query(in q, (ref Entity e) =>
        {
            var list = new List<INetworkableComponent>(4);

            foreach (var t in NetworkableTypes)
            {
                if (!e.Has(t)) continue;

                if (e.Get(t) is INetworkableComponent net)
                    list.Add(net);
            }

            if (list.Count > 0)
            {
                result[e.Id] = [.. list];
            }
        });

        return result;
    }
}
