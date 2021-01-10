using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Data;
using Data.ScriptableObjects;
using TerrainGeneration.Utils;
using TerrainGeneration;

namespace Controllers
{
    public class HomeScreenController : MonoBehaviour
    {
        public Material meshMaterial;
        public Light sunlight;
        public Button continueButton;
        public TerrainParams[] terrains;

        private GameObject meshObject;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private TerrainParams activeTerrainParams;
        private readonly System.Random prng = new System.Random();

        private void Start()
        {
            SetContinueButtonActiveState();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach (var terrain in terrains)
            {
                terrain.isActive = false;
            }

            meshObject = new GameObject($"Preview mesh");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshRenderer.material = meshMaterial;
            meshObject.transform.position = Vector3.zero;
            meshObject.transform.Rotate(-30f, 0f, 0f, Space.Self);
        }

        private void Update()
        {
            meshObject.transform.Rotate(0f, 10f * Time.deltaTime, 0f, Space.Self);
        }

        public void PlayGame()
        {
            SceneManager.LoadScene("MainScene");
        }

        public void SetTerrainParams(TerrainParams param)
        {
            if (activeTerrainParams != null)
            {
                activeTerrainParams.isActive = false;
            }

            activeTerrainParams = param;
            activeTerrainParams.noiseParams.seed = 0;
            activeTerrainParams.isActive = true;
            GeneratePreview();
        }

        public void SetContinueButtonActiveState()
        {
            if (PlayerPrefs.HasKey(Config.PrefKeyGameSave))
            {
                string json = PlayerPrefs.GetString(Config.PrefKeyGameSave);
                continueButton.interactable = !string.IsNullOrEmpty(json);
            }
        }

        public void SetContinueGamePrefFlag(bool value)
        {
            PlayerPrefs.SetInt(Config.PrefKeyContiuneGameFlag, value ? 1 : 0);
        }

        private void GeneratePreview()
        {
            InitLightingFromParams();
            var noiseMap = NoiseGenerator.GenerateFromPerlinNoise(
                MapGenerator.mapChunkVerticesPerLineWithBorder,
                activeTerrainParams.noiseParams,
                0, 0,
                prng.Next(),
                NormalizationType.Local);

            var meshData = MeshGenerator.GenerateFromNoiseMap(noiseMap, MapGenerator.mapChunkVerticesPerLine, activeTerrainParams, (int)LevelOfDetail._1);
            meshFilter.sharedMesh = meshData.GetUnityMesh();
        }

        private void InitLightingFromParams()
        {
            RenderSettings.fog = activeTerrainParams.fogEnabled;
            RenderSettings.fogDensity = activeTerrainParams.fogDensity;
            RenderSettings.fogColor = activeTerrainParams.fogColor;
            RenderSettings.skybox = activeTerrainParams.skybox;
            sunlight.color = activeTerrainParams.sunlightColor;
            sunlight.intensity = activeTerrainParams.sunlightIntensity;
        }
    }
}
