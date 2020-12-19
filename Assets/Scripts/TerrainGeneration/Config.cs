namespace TerrainGeneration
{
    public static class InfiniteTerrainConfig
    {
        public const int chunkSize = 240;
        public const int maxGridCoord = 4;
        public const int worldHalfWidth = chunkSize * maxGridCoord;
    }

    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 180),
            new LodDistance( LevelOfDetail._2, 250),
            new LodDistance( LevelOfDetail._4, 500),
            new LodDistance( LevelOfDetail._24, 2000),
        };

        public const float playerPositionThresholdForChunksUpdate = InfiniteTerrainConfig.chunkSize / 2f;
        public static readonly float maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance;
        public static int lodDistanceIndexForCollider = 1;
    }
}
