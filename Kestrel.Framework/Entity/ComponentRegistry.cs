using System.Reflection;

namespace Kestrel.Framework.Entity;

public class ComponentRegistry
{
    INetworkableComponent[] components;
    string targetNamespace = "Kestrel.Framework.Entity.Components";

    public ComponentRegistry()
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsValueType &&
                t.IsAssignableTo(typeof(INetworkableComponent)) &&
                t.Namespace == targetNamespace &&
                t.IsDefined(typeof(System.Runtime.CompilerServices.IsReadOnlyAttribute), false) // heuristic for record struct
            )
            .ToList();
    }

    public void asd()
    {

        var components =
    }
}