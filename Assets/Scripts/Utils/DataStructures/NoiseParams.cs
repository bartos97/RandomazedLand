using UnityEngine;

namespace Utils.DataStructures
{
    [System.Serializable]
    public struct NoiseParams
    {
        public const float DefaultOffsetX = 0f;
        public const float DefaultOffsetY = 0f;
        public const float DefaultScale = 1f;
        public const float DefaultPersistance = 1f;
        public const float DefaultLacunarity = 1f;
        public const int DefaultOctavesAmount = 1;

        public static NoiseParams CreateWithDefaults()
        {
            return new NoiseParams
            {
                OffsetX = DefaultOffsetX,
                OffsetY = DefaultOffsetY,
                Scale = DefaultScale,
                Persistance = DefaultPersistance,
                Lacunarity = DefaultLacunarity,
                OctavesAmount = DefaultOctavesAmount
            };
        }

        public float OffsetX;
        public float OffsetY;
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
