using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New noise params", menuName = "Terrain generation/Noise parameters")]
    public class NoiseParams : UpdatableData
    {
        public int seed;
        [Min(1f)]
        public float scale;
        [Range(0, 1)]
        public float persistance;
        [Range(1, 10)]
        public float lacunarity;
        [Range(1, 10)]
        public int octavesAmount;
    }
}
