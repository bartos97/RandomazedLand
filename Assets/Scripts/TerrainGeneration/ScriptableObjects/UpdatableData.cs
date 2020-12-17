using System;
using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    public class UpdatableData : ScriptableObject
    {
        public event EventHandler ValuesUpdated;
        public bool autoUpdate;

        public void RaiseValuesUpdatedEvent()
        {
            ValuesUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
