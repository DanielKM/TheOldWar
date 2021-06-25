using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    public class LowPolyHelper : MonoBehaviour
    {
        public static void FlattenPolysAndUVs(ref Vector3[] flatVertices, ref Vector2[] flatUVs, ref int[] polys, Vector3[] inputVertices, Vector2[] inputUVs)
        {
            flatVertices = new Vector3[polys.Length];
            flatUVs = new Vector2[polys.Length];
            int numPolys = polys.Length - 1;
            for (int i = numPolys; i >= 0; i--)
            {
                flatVertices[i] = inputVertices[polys[i]];
                flatUVs[i] = inputUVs[polys[i]];
                polys[i] = numPolys - i;
            }
        }
        

        public static Color[] BakeSharpVertexColorsToArray(Vector3[] flatVertices, int[] tPolys, Color[] textureColors, int mipWidth, Terrain terrain)
        {
            Color[] returnArray = new Color[flatVertices.Length];
            Color color1 = Color.white;
            Color color2 = Color.white;
            Color color3 = Color.white;
            Color finalColor = Color.white;
            Vector3 colorVertex = Vector3.zero;
            int numPolys = tPolys.Length-1;
            for (int i = 0; i <= numPolys; i += 3)
            {
                colorVertex = flatVertices[tPolys[i]];
                color1 = GetColorFromTextureArray(textureColors, mipWidth, terrain, colorVertex);

                colorVertex = flatVertices[tPolys[i + 1]];
                color2 = GetColorFromTextureArray(textureColors, mipWidth, terrain, colorVertex);

                colorVertex = flatVertices[tPolys[i + 2]];
                color3 = GetColorFromTextureArray(textureColors, mipWidth, terrain, colorVertex);

                finalColor = (color1 + color2 + color3) / 3f;

                returnArray[tPolys[i]] = finalColor;
                returnArray[tPolys[i + 1]] = finalColor;
                returnArray[tPolys[i + 2]] = finalColor;
            }
            return returnArray;
        }

        public static Color[] BakeSmoothVertexColorsToArray(Vector3[] vertices, Color[] textureColors, int mipWidth, Terrain terrain)
        {
            int numVertices = vertices.Length;
            Color[] returnArray = new Color[vertices.Length];
            Color color = Color.white;
            Vector3 colorVertex = Vector3.zero;
            for (int i = 0; i < numVertices; i++)
            {
                colorVertex = vertices[i];
                color = GetColorFromTextureArray(textureColors, mipWidth, terrain, colorVertex);
                returnArray[i] = color;
            }
            return returnArray;
        }

        private static Color GetColorFromTextureArray(Color[] textureColors, int mipWidth, Terrain terrain, Vector3 colorVertex)
        {
            int textureXPos = Math.Max(0, Mathf.RoundToInt((colorVertex.x / terrain.terrainData.size.x) * mipWidth) - 1);
            int textureZPos = Math.Max(0, Mathf.RoundToInt((colorVertex.z / terrain.terrainData.size.z) * mipWidth) - 1);
            return textureColors[Math.Max(0, textureZPos * mipWidth + textureXPos)];
        }
    }
}