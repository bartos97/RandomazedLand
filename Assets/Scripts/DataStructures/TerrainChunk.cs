using UnityEngine;

namespace DataStructures
{
    public class TerrainChunk
    {
        private readonly GameObject mesh;
        private Vector3 positionInWorld;
        private Bounds positionBounds;
        private bool _isVisible;

        public TerrainChunk(Vector2 gridCoords, int size, Transform parentObject)
        {
            positionInWorld = new Vector3(gridCoords.x * size, 0f, gridCoords.y * size);
            positionBounds = new Bounds(positionInWorld, Vector3.one * size);

            mesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
            mesh.transform.position = positionInWorld;
            mesh.transform.localScale = Vector3.one * size / 10f; //plane's default scale is 10
            mesh.transform.parent = parentObject;
            mesh.SetActive(true);
        }

        public bool IsVisible 
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                mesh.SetActive(value);
            }
        }

        public void UpdateVisibility(Vector3 playerPosition, float maxViewDistance)
        {
            float distanceFromPlayer = positionBounds.SqrDistance(playerPosition);
            IsVisible = distanceFromPlayer <= maxViewDistance * maxViewDistance; //distanceFromPlayer is squared
        }
    }
}
