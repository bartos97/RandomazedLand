namespace Data
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
        _8 = 8,
        _10 = 10,
        _12 = 12,
        _24 = 24,
        _48 = 48,
        _80 = 80,
        _120 = 120
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
