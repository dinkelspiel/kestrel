namespace Kestrel.Framework.Client.Graphics.Buffers;

public class ChunkMeshManager
{
    readonly List<ChunkMesh> chunkMeshGenerationQueue = [];


    public void QueueGeneration(ChunkMesh chunkMesh)
    {
        chunkMeshGenerationQueue.Add(chunkMesh);
    }

    public void GenerateFromQueueUnderTimeLimit(int milliseconds)
    {
        DateTime started = DateTime.Now;

        TimeSpan elapsed = DateTime.Now - started;
        while (elapsed.Milliseconds < milliseconds && chunkMeshGenerationQueue.Count > 0)
        {
            elapsed = DateTime.Now - started;
            ChunkMesh chunkMesh = chunkMeshGenerationQueue[0];
            chunkMeshGenerationQueue.RemoveAt(0);
            chunkMesh.Generate();
        }
    }
}