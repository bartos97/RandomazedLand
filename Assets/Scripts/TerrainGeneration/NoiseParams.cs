using UnityEngine;

namespace TerrainGeneration
{
    [System.Serializable]
    public struct NoiseParams
    {
        public const float DefaultScale = 1f;
        public const float DefaultPersistance = 1f;
        public const float DefaultLacunarity = 1f;
        public const int DefaultOctavesAmount = 1;

        public static NoiseParams CreateWithDefaults()
        {
            return new NoiseParams
            {
                Scale = DefaultScale,
                Persistance = DefaultPersistance,
                Lacunarity = DefaultLacunarity,
                OctavesAmount = DefaultOctavesAmount
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
