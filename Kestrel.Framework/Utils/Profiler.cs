using System.Text;

public class Profiler
{
    public long Tick = 0;
    public Dictionary<long, Dictionary<string, int>> Log = [];
    public List<string> Labels = [];
    public bool Enabled = false;

    public void Start(String label, Action action)
    {
        if (!Enabled)
        {
            action();
            return;
        }
        DateTime start = DateTime.Now;

        action();

        TimeSpan duration = DateTime.Now - start;

        if (!Labels.Contains(label))
            Labels.Add(label);

        Dictionary<string, int> TickDict; ;
        if (!Log.ContainsKey(Tick))
        {
            TickDict = [];
            Log.Add(Tick, TickDict);
        }
        else
        {
            TickDict = Log[Tick];
        }

        if (!TickDict.ContainsKey(label))
        {
            TickDict.Add(label, duration.Milliseconds);
        }
        else
        {
            TickDict.Add(label + new Random().NextDouble() * 10, duration.Milliseconds);
        }
    }

    public void Build()
    {
        if (!Enabled)
            return;

        StringBuilder sb = new();
        String line = "";
        foreach (var label in Labels)
        {
            line += $",{label}";
        }
        sb.AppendLine(line);

        foreach (var kvp in Log)
        {
            line = $"{kvp.Key}";
            foreach (var label in Labels)
            {
                if (kvp.Value.TryGetValue(label, out var log))
                {
                    line += $",{log}";
                }
                else
                {
                    line += ",";
                }
            }
            sb.AppendLine(line);
        }

        File.WriteAllText("./log.txt", sb.ToString());
    }
}