namespace TerrainGeneration
{
    public static class LevelOfDetailConfig
    {
        public static readonly LodDistance[] distanceThresholds = {
            new LodDistance( LevelOfDetail._1, 150),
            new LodDistance( LevelOfDetail._2, 250),
            new LodDistance( LevelOfDetail._4, 400),
            new LodDistance( LevelOfDetail._8, 600),
        };

        public const int chunkSize = 240;
        public const float playerPositionThresholdForChunksUpdate = chunkSize / 2f;
        public static readonly float maxViewDistance = distanceThresholds[distanceThresholds.Length - 1].viewDistance / terrainScale;
        public const float terrainScale = 1.0f;
    }
}
