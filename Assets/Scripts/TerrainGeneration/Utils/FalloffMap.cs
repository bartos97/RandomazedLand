using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace TerrainGeneration.Utils
{
    public enum BorderChunkType
    {
        Invalid,
        BottomLeft,
        BottomMiddle,
        BottomRight,
        MiddleLeft,
        MiddleRight,
        TopLeft,
        TopMiddle,
        TopRight
    }

    public class FalloffMap
    {
        public float[] bottomLeftChunk;
        public float[] bottomMiddleChunk;
        public float[] bottomRightChunk;
        public float[] middleLeftChunk;
        public float[] middleRightChunk;
        public float[] topLeftChunk;
        public float[] topMiddleChunk;
        public float[] topRightChunk;

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

            bottomLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, 0, bottomLeftChunk);
            bottomMiddleChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine, 0, bottomMiddleChunk);
            bottomRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, 0, bottomRightChunk);

            middleLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, chunkPointsPerLine, middleLeftChunk);
            middleRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, chunkPointsPerLine, middleRightChunk);

            topLeftChunk = new float[chunkElemsCount];
            CreateChunk(0, chunkPointsPerLine * 2, topLeftChunk);
            topMiddleChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine, chunkPointsPerLine * 2, topMiddleChunk);
            topRightChunk = new float[chunkElemsCount];
            CreateChunk(chunkPointsPerLine * 2, chunkPointsPerLine * 2, topRightChunk);
        }

        public float[] CombinedMap
        {
            get
            {
                if (!isGenerated)
                {
                    combinedMap = new float[chunkElemsCount];

                    FromChunkToCombined(0, 0, bottomLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk, 0, bottomMiddleChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, 0, bottomRightChunk);

                    FromChunkToCombined(0, oneThirdOfChunk, middleLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, oneThirdOfChunk, middleRightChunk);

                    FromChunkToCombined(0, oneThirdOfChunk * 2, topLeftChunk);
                    FromChunkToCombined(oneThirdOfChunk, oneThirdOfChunk * 2, topMiddleChunk);
                    FromChunkToCombined(oneThirdOfChunk * 2, oneThirdOfChunk * 2, topRightChunk);

                    isGenerated = true;
                }

                return combinedMap;
            }
        }

        public float[] GetChunk(BorderChunkType type)
        {
            switch (type)
            {
                case BorderChunkType.BottomLeft:
                    return bottomLeftChunk;
                case BorderChunkType.BottomMiddle:
                    return bottomMiddleChunk;
                case BorderChunkType.BottomRight:
                    return bottomRightChunk;
                case BorderChunkType.MiddleLeft:
                    return middleLeftChunk;
                case BorderChunkType.MiddleRight:
                    return middleRightChunk;
                case BorderChunkType.TopLeft:
                    return topLeftChunk;
                case BorderChunkType.TopMiddle:
                    return topMiddleChunk;
                case BorderChunkType.TopRight:
                    return topRightChunk;

                case BorderChunkType.Invalid:
                default:
                    throw new ArgumentException("Invalid border chunk type");
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
            float xValue = x / (float)samplerPointsPerLine * 2 - 1;
            float yValue = y / (float)samplerPointsPerLine * 2 - 1;
            float val = Mathf.Max(Mathf.Abs(xValue), Mathf.Abs(yValue));
            return EvaluateValue(val);
        }

        private float EvaluateValue(float x)
        {
            return x >= 0.5f ? 4*x*x - 4*x + 1 : 0f;
            //const float a = 3.0f;
            //const float b = 6.0f;
            //return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(b - b * x, a));
        }
    }
}
