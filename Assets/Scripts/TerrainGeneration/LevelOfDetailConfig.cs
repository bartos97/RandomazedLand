namespace TerrainGeneration
{
    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 250),
            new LodDistance( LevelOfDetail._2, 400),
            new LodDistance( LevelOfDetail._4, 600),
            new LodDistance( LevelOfDetail._8, 800),
        };

        public const int chunkSize = 240;
        public const float playerPositionThresholdForChunksUpdate = chunkSize / 4f;
        public static readonly float maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance;
        public static int lodDistanceIndexForCollider = 1;
    }
}
