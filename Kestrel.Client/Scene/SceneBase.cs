namespace Kestrel.Client.Scene;

public abstract class SceneBase(ClientContext clientContext)
{
    readonly ClientContext clientContext = clientContext;

    public abstract void Load();
    public abstract void Update(double dt);
    public abstract void Render(double dt);
    public virtual void Unload() { }
}