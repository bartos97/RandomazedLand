using UnityEngine;

namespace TerrainGeneration.Structs
{
    [System.Serializable]
    public struct NoiseParams
    {
        public static NoiseParams CreateWithDefaults()
        {
            return new NoiseParams
            {
                Scale = 1.0f,
                Persistance = 1.0f,
                Lacunarity = 1.0f,
                OctavesAmount = 1
            };
        }

        [Min(1f)]
        public float Scale;
        [Range(0, 1)]
        public float Persistance;
        [Range(1, 10)]
        public float Lacunarity;
        [Range(1, 10)]
        public int OctavesAmount;
    }
}
