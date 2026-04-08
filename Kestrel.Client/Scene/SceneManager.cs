using Kestrel.Client.Scenes;

namespace Kestrel.Client.Scene;

public record SceneKind(Func<ClientContext, SceneBase> Factory)
{
    public static readonly SceneKind MainMenu = new(ctx => new MainMenuScene(ctx));
    public static readonly SceneKind Game = new(ctx => new GameScene(ctx));
}

public class SceneManager(ClientContext clientContext)
{
    public SceneBase activeScene = SceneKind.MainMenu.Factory(clientContext);
    public ClientContext clientContext = clientContext;

    public void SetActiveScene(SceneKind kind)
    {
        activeScene.Unload();
        activeScene = kind.Factory(clientContext);
        activeScene.Load();
    }
}