using UnityEngine;

namespace TerrainGeneration.Structs
{
    [System.Serializable]
    public struct TerrainParams
    {
        [Min(1)]
        public int DepthMultiplier;
        [Min(0f)]
        public float UniformScaleMultiplier;
        public Gradient ColorRegions;
        public AnimationCurve MeshHeightCurve;
    }

}
