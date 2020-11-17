using System.Collections.Generic;
using DataStructures;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration
{
    public class InfiniteTerrain : MonoBehaviour
    {
        public Transform player;

        private const float maxViewDistance = 250f;
        private const int chunkSize = MapGenerator.MapChunkVerticesCount - 1;
        private const int numOfVisibleChunksInViewDirection = (int)(maxViewDistance / chunkSize) + (maxViewDistance % chunkSize >= chunkSize / 2 ? 1 : 0); //round to closest int

        private readonly Dictionary<Vector2, TerrainChunk> chunksRepository = new Dictionary<Vector2, TerrainChunk>();
        private readonly List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

        private void Start()
        {
            Assert.IsTrue(maxViewDistance > chunkSize);
            player.position = Vector3.zero;
        }

        private void Update()
        {
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            ClearLastChunks();
            int xMiddleChunkCoord = Mathf.RoundToInt(player.position.x / chunkSize);
            int yMiddleChunkCoord = Mathf.RoundToInt(player.position.z / chunkSize);

            for (int yChunkCoordIter = -numOfVisibleChunksInViewDirection; yChunkCoordIter <= numOfVisibleChunksInViewDirection; yChunkCoordIter++)
            {
                for (int xChunkCoordIter = -numOfVisibleChunksInViewDirection; xChunkCoordIter <= numOfVisibleChunksInViewDirection; xChunkCoordIter++)
                {
                    var chunkCoords = new Vector2(xMiddleChunkCoord + xChunkCoordIter, yMiddleChunkCoord + yChunkCoordIter);

                    if (chunksRepository.ContainsKey(chunkCoords))
                    {
                        chunksRepository[chunkCoords].UpdateVisibility(player.position, maxViewDistance);
                        if (chunksRepository[chunkCoords].IsVisible)
                        {
                            lastVisibleChunks.Add(chunksRepository[chunkCoords]);
                        }
                    }
                    else
                    {
                        chunksRepository.Add(chunkCoords, new TerrainChunk(chunkCoords, chunkSize, this.transform));
                    }
                }
            }
        }

        private void ClearLastChunks()
        {
            foreach (var chunk in lastVisibleChunks)
                chunk.IsVisible = false;
            lastVisibleChunks.Clear();
        }
    }
}
