using UnityEngine;

namespace Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New terrain params", menuName = "Terrain generation parameters")]
    public class TerrainParams : UpdatableData
    {
        [Space]
        public bool isActive = false;
        public NoiseParams noiseParams;

        [Header("Terrain")]
        public bool useFalloffMap;
        [Min(1)]
        public int DepthMultiplier;
        [Min(0f)]
        public float UniformScaleMultiplier;
        public Gradient ColorRegions;
        public AnimationCurve MeshHeightCurve;

        [Header("Environment")]
        public bool fogEnabled;
        public Color fogColor;
        public float fogDensity;
        public Material skybox;
        public Color sunlightColor;
        public float sunlightIntensity;
    }

}
