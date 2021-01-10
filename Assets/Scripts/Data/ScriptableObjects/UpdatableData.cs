using System;
using UnityEngine;

namespace Data.ScriptableObjects
{
    public class UpdatableData : ScriptableObject
    {
        public event Action ValuesUpdated;
        public bool autoUpdate;

        public void RaiseValuesUpdatedEvent()
        {
            ValuesUpdated?.Invoke();
        }
    }
}
