namespace TerrainGeneration
{
    /// <summary>
    /// Mesh simplification increment
    /// </summary>
    public enum LevelOfDetail
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _6 = 6,
        _8 = 8
    }

    public struct LodDistance
    {
        public LevelOfDetail lod;
        public float viewDistance;

        public LodDistance(LevelOfDetail lod, float viewDistance)
        {
            this.lod = lod;
            this.viewDistance = viewDistance;
        }
    }
}
