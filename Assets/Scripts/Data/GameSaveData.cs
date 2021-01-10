using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class GameSaveData
    {
        public int seed;
        public string terrainName;
        public float[] playerPos;
    }
}
