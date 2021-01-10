using UnityEngine;

namespace Data.ScriptableObjects
{
    [System.Serializable]
    public class NoiseParams
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
