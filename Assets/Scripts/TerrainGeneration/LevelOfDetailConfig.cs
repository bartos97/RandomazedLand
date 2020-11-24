namespace TerrainGeneration
{
    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 250),
            new LodDistance( LevelOfDetail._2, 500),
            new LodDistance( LevelOfDetail._4, 750),
        };

        public const int chunkSize = 240;
        public const float playerPositionThresholdForChunksUpdate = chunkSize / 2f;
        public static readonly float maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance;
    }
}
