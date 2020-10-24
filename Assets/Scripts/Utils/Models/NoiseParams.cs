using UnityEngine;

namespace Utils.Models
{
    [System.Serializable]
    public class NoiseParams
    {
        public const float DefaultOffsetX = 0f;
        public const float DefaultOffsetY = 0f;
        public const float DefaultScale = 1f;
        public const float DefaultPersistance = 1f;
        public const float DefaultLacunarity = 1f;
        public const int DefaultOctavesAmount = 1;

        public float OffsetX = DefaultOffsetX;
        public float OffsetY = DefaultOffsetY;

        [Min(1f)]
        public float Scale = DefaultScale;

        [Range(0, 1)]
        public float Persistance = DefaultPersistance;

        [Range(1, 10)]
        public float Lacunarity = DefaultLacunarity;

        [Range(1, 10)]
        public int OctavesAmount = DefaultOctavesAmount;
    }
}
