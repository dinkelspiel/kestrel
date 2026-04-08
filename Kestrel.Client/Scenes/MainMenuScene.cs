using System.Numerics;
using ImGuiNET;
using Kestrel.Client.Scene;
using Silk.NET.Input;

namespace Kestrel.Client.Scenes;

public class MainMenuScene(ClientContext clientContext) : SceneBase(clientContext)
{
    public override void Load()
    {
        clientContext.Mouse.Cursor.CursorMode = CursorMode.Normal;
    }

    public override void Render(double dt)
    {
        var viewport = ImGui.GetMainViewport();
        var center = viewport.GetCenter();

        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(300, 200));

        ImGui.Begin("Main Menu", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);

        var titleSize = ImGui.CalcTextSize("Kestrel");
        ImGui.SetCursorPosX((300 - titleSize.X) / 2);
        ImGui.Text("Kestrel");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        float buttonWidth = 200;
        float buttonHeight = 40;
        ImGui.SetCursorPosX((300 - buttonWidth) / 2);
        if (ImGui.Button("Play", new Vector2(buttonWidth, buttonHeight)))
        {
            clientContext.sceneManager.SetActiveScene(SceneKind.Game);
        }

        ImGui.End();
    }

    public override void Update(double dt)
    {
    }
}