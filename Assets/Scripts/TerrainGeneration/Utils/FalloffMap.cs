using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration.Utils
{
    public class FalloffMap
    {
        public float[] BottomLeftChunk { get; private set; }
        public float[] BottomMiddleChunk {get; private set;}
        public float[] BottomRightChunk { get; private set;}
        public float[] MiddleLeftChunk { get; private set;}
        public float[] MiddleRightChunk { get; private set;}
        public float[] TopLeftChunk { get; private set;}
        public float[] TopMiddleChunk { get; private set;}
        public float[] TopRightChunk { get; private set;}

        private readonly int chunkElemsCount;
        private readonly int chunkPointsPerLine;
        private readonly int oneThirdOfChunk;
        private readonly int samplerPointsPerLine;
        private bool isGenerated = false;
        private float[] combinedMap;

        public FalloffMap(int chunkPointsPerLine)
        {
            Assert.IsTrue(chunkPointsPerLine % 3 == 0, "Number of points per line in falloff map has to be divisible by 3");

            this.chunkPointsPerLine = chunkPointsPerLine;
            chunkElemsCount = chunkPointsPerLine * chunkPointsPerLine;
            oneThirdOfChunk = chunkPointsPerLine / 3;
            samplerPointsPerLine = chunkPointsPerLine * 3;

            BottomLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, 0, BottomLeftChunk);
            BottomMiddleChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine, 0, BottomMiddleChunk);
            BottomRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, 0, BottomRightChunk);

            MiddleLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, chunkPointsPerLine, MiddleLeftChunk);
            MiddleRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, chunkPointsPerLine, MiddleRightChunk);

            TopLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, chunkPointsPerLine * 2, TopLeftChunk);
            TopMiddleChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine, chunkPointsPerLine * 2, TopMiddleChunk);
            TopRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, chunkPointsPerLine * 2, TopRightChunk);
        }

        public float[] CombinedMap
        {
            get
            {
                if (!isGenerated)
                {
                    combinedMap = new float[chunkElemsCount];

                    FromChunkToCombined(0, 0, BottomLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk, 0, BottomMiddleChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, 0, BottomRightChunk);

                    FromChunkToCombined(0, oneThirdOfChunk, MiddleLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, oneThirdOfChunk, MiddleRightChunk);

                    FromChunkToCombined(0, oneThirdOfChunk * 2, TopLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk, oneThirdOfChunk * 2, TopMiddleChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, oneThirdOfChunk * 2, TopRightChunk);

                    isGenerated = true;
                }

                return combinedMap;
            }
        }

        private void FromChunkToCombined(int xStart, int yStart, float[] chunk)
        {
            int yChunk = 0;
            for (int y = yStart; y < yStart + oneThirdOfChunk; y++)
            {
                int xChunk = 0;
                for (int x = xStart; x < xStart + oneThirdOfChunk; x++)
                {
                    combinedMap[x + y * chunkPointsPerLine] = chunk[xChunk + yChunk * chunkPointsPerLine];
                    xChunk += 3;
                }
                yChunk += 3;
            }
        }

        private void CreateChunk(int xStartSampler, int yStartSampler, float[] chunk)
        {
            int chunkIndex = 0;
            for (int y = 0; y < chunkPointsPerLine; y++)
            {
                for (int x = 0; x < chunkPointsPerLine; x++)
                {
                    chunk[chunkIndex++] = SampleValue(xStartSampler + x, yStartSampler + y);
                }
            }
        }

        private float SampleValue(int x, int y)
        {
            //var pos = new Vector2(x - (float)samplerPointsPerLine / 2, y - (float)samplerPointsPerLine / 2);
            //var bounds = new Bounds(Vector3.zero, new Vector3(chunkPointsPerLine / 2, chunkPointsPerLine / 2, chunkPointsPerLine / 2));
            //return Mathf.InverseLerp(0f, chunkPointsPerLine * 1.5f * chunkPointsPerLine * 1.5f, bounds.SqrDistance(pos));

            float xValue = x / (float)samplerPointsPerLine * 2 - 1;
            float yValue = y / (float)samplerPointsPerLine * 2 - 1;
            float val = Mathf.Max(Mathf.Abs(xValue), Mathf.Abs(yValue));
            return EvaluateValue(val);
        }

        private float EvaluateValue(float x)
        {
            return x >= 0.5f ? 4 * (x - 0.5f) * (x - 0.5f) : 0f;
            //const float a = 3.0f;
            //const float b = 6.0f;
            //return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(b - b * x, a));
        }
    }
}
