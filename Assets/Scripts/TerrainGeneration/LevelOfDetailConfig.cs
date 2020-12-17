namespace TerrainGeneration
{
    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 180),
            new LodDistance( LevelOfDetail._2, 250),
            new LodDistance( LevelOfDetail._4, 800),
            new LodDistance( LevelOfDetail._24, 2000),
            new LodDistance( LevelOfDetail._80, 5000),
        };

        public const int chunkSize = 240;
        public const float playerPositionThresholdForChunksUpdate = chunkSize / 2f;
        public static readonly float maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance;
        public static int lodDistanceIndexForCollider = 1;
    }
}
