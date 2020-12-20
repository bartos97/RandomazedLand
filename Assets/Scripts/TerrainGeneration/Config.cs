namespace TerrainGeneration
{
    public static class InfiniteTerrainConfig
    {
        public const int chunkSize = 240;
        public const int maxChunkGridCoord = 9;
        public const int worldHalfWidth = chunkSize * maxChunkGridCoord;
    }

    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 180),
            new LodDistance( LevelOfDetail._2, 250),
            new LodDistance( LevelOfDetail._4, 500),
            new LodDistance( LevelOfDetail._24, 1000),
            new LodDistance( LevelOfDetail._80, 2000),
        };

        public static readonly LodDistance[] distanceThresholdsFaloffMap = {
            new LodDistance( LevelOfDetail._1, 180),
            new LodDistance( LevelOfDetail._2, 250),
            new LodDistance( LevelOfDetail._4, 500),
            new LodDistance( LevelOfDetail._10, 2000),
            new LodDistance( LevelOfDetail._120, 6000),
        };

        public const float playerPositionThresholdForChunksUpdate = InfiniteTerrainConfig.chunkSize / 2f;
        public static int lodDistanceIndexForCollider = 1;
    }
}
