using UnityEngine;

namespace TerrainGeneration.DataStructures
{
    public class TerrainChunk
    {
        private readonly GameObject meshGameObject;
        private Vector3 positionInWorld;
        private Bounds positionBounds;

        public TerrainChunk(Vector2 gridCoords, int size, Transform parentObjectTransform)
        {
            positionInWorld = new Vector3(gridCoords.x * size, 0f, gridCoords.y * size);
            positionBounds = new Bounds(positionInWorld, Vector3.one * size);

            meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshGameObject.transform.position = positionInWorld;
            meshGameObject.transform.localScale = Vector3.one * size / 10f; //plane's default scale is 10
            meshGameObject.transform.parent = parentObjectTransform;
            meshGameObject.SetActive(true);
        }

        public bool IsVisible { get; private set; }

        public void UpdateVisibility(Vector3 playerPosition, float maxViewDistance)
        {
            float distanceFromPlayer = positionBounds.SqrDistance(playerPosition);
            IsVisible = distanceFromPlayer <= maxViewDistance * maxViewDistance; //distanceFromPlayer is squared
            SetVisibility(IsVisible); 
        }

        public void SetVisibility(bool value)
        {
            meshGameObject.SetActive(value);
        }
    }
}
