﻿using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New terrain params", menuName = "Terrain generation/Terrain parameters")]
    public class TerrainParams : ScriptableObject
    {
        public bool useFalloffMap;
        [Min(1)]
        public int DepthMultiplier;
        [Min(0f)]
        public float UniformScaleMultiplier;
        public Gradient ColorRegions;
        public AnimationCurve MeshHeightCurve;
    }

}