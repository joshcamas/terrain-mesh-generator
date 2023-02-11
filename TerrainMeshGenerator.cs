namespace SpellcastStudios
{
    using UnityEngine;

    public class TerrainChunkExporter
    {
        public static Mesh ConvertTerrainToMesh(TerrainData terrain, int terrainQuality)
        {
            int w = terrain.heightmapResolution;
            int h = terrain.heightmapResolution;
            Vector3 meshScale = terrain.size;

            return ConvertTerrainToMesh(w, h, terrain.GetHeights(0, 0, w, h), meshScale, terrainQuality);
        }

        public static Mesh ConvertTerrainToMesh(int w, int h, float[,] heights, Vector3 meshScale, int terrainQuality)
        {
            var combInst1 = new CombineInstance();
            combInst1.mesh = GetTerrainMesh(w, h, heights, meshScale, terrainQuality);

            var combInst2 = new CombineInstance();
            combInst2.mesh = GetEdgeMesh(w, h, heights, meshScale, terrainQuality);

            Mesh combMesh = new Mesh();
            combMesh.CombineMeshes(new CombineInstance[] { combInst1, combInst2 }, true, false);

            return combMesh;
        }

        public static Mesh GetTerrainMesh(int w, int h, float[,] heights, Vector3 meshScale, int terrainQuality)
        {
            float tRes = Mathf.Pow(2, terrainQuality);

            meshScale = new Vector3(meshScale.x / (w - Mathf.Pow(2, terrainQuality)) * tRes, meshScale.y, meshScale.z / (h - Mathf.Pow(2, terrainQuality)) * tRes);
            var uvScale = new Vector2(1.0f / ((float)w - Mathf.Pow(2, terrainQuality)), 1.0f / ((float)h - Mathf.Pow(2, terrainQuality)));

            w = (int)((float)(w - 1) / tRes + 1);
            h = (int)((float)(h - 1) / tRes + 1);
            var tVertices = new Vector3[w * h];
            var tUV = new Vector2[w * h];

            int[] tPolys = new int[(w - 1) * (h - 1) * 6];

            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)], x));
                    tUV[y * w + x] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);
                }
            }

            var index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = tVertices;

            mesh.triangles = tPolys;
            mesh.uv = tUV;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            System.Array.Reverse(tPolys);

            mesh.triangles = tPolys;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static Mesh GetEdgeMesh(int w, int h, float[,] heights, Vector3 meshScale, int terrainQuality)
        {
            float edgeHeight = .1f;

            float tRes = Mathf.Pow(2, terrainQuality);

            //I have absolutely no idea what this line does
            meshScale = new Vector3(meshScale.x / (w - Mathf.Pow(2, terrainQuality)) * tRes, meshScale.y, meshScale.z / (h - Mathf.Pow(2, terrainQuality)) * tRes);
            var uvScale = new Vector2(1.0f / ((float)w - Mathf.Pow(2, terrainQuality)), 1.0f / ((float)h - Mathf.Pow(2, terrainQuality)));

            w = (int)((float)(w - 1) / tRes + 1);
            h = (int)((float)(h - 1) / tRes + 1);

            var tVertices = new Vector3[h * 8 + w * 8];
            var tUV = new Vector2[h * 8 + w * 8];

            int[] tPolys = new int[(h * 8 + w * 8) * 6];

            for (int y = 0; y < h - 1; y++)
            {
                int x = 0;

                tVertices[y * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)], x));
                tUV[y * 4] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 1] = Vector3.Scale(meshScale, new Vector3(y + 1, heights[(int)(x * tRes), (int)((y + 1) * tRes)], x));
                tUV[y * 4 + 1] = Vector2.Scale(new Vector2((y + 1) * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 2] = Vector3.Scale(meshScale, new Vector3(y + 1, heights[(int)(x * tRes), (int)((y + 1) * tRes)] * -edgeHeight, x));
                tUV[y * 4 + 2] = Vector2.Scale(new Vector2((y + 1) * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 3] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)] * -edgeHeight, x));
                tUV[y * 4 + 3] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                x = w - 1;

                tVertices[y * 4 + h * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)] * -edgeHeight, x));
                tUV[y * 4 + h * 4] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 1 + h * 4] = Vector3.Scale(meshScale, new Vector3(y + 1, heights[(int)(x * tRes), (int)((y + 1) * tRes)] * -edgeHeight, x));
                tUV[y * 4 + 1 + h * 4] = Vector2.Scale(new Vector2((y + 1) * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 2 + h * 4] = Vector3.Scale(meshScale, new Vector3(y + 1, heights[(int)(x * tRes), (int)((y + 1) * tRes)], x));
                tUV[y * 4 + 2 + h * 4] = Vector2.Scale(new Vector2((y + 1) * tRes, x * tRes), uvScale);

                tVertices[y * 4 + 3 + h * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)], x));
                tUV[y * 4 + 3 + h * 4] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

            }

            for (int x = 0; x < h - 1; x++)
            {
                int y = 0;

                tVertices[x * 4 + h * 8] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)], x));
                tUV[x * 4 + h * 8] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 1] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)] * -edgeHeight, x));
                tUV[x * 4 + h * 8 + 1] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 2] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)((x + 1) * tRes), (int)(y * tRes)] * -edgeHeight, x + 1));
                tUV[x * 4 + h * 8 + 2] = Vector2.Scale(new Vector2(y * tRes, (x + 1) * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 3] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)((x + 1) * tRes), (int)(y * tRes)], x + 1));
                tUV[x * 4 + h * 8 + 3] = Vector2.Scale(new Vector2(y * tRes, (x + 1) * tRes), uvScale);


                y = h - 1;
                tVertices[x * 4 + h * 8 + w * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)], x));
                tUV[x * 4 + h * 8 + w * 4] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 1 + w * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)((x + 1) * tRes), (int)(y * tRes)], x + 1));
                tUV[x * 4 + h * 8 + 1 + w * 4] = Vector2.Scale(new Vector2(y * tRes, (x + 1) * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 2 + w * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)((x + 1) * tRes), (int)(y * tRes)] * -edgeHeight, x + 1));
                tUV[x * 4 + h * 8 + 2 + w * 4] = Vector2.Scale(new Vector2(y * tRes, (x + 1) * tRes), uvScale);

                tVertices[x * 4 + h * 8 + 3 + w * 4] = Vector3.Scale(meshScale, new Vector3(y, heights[(int)(x * tRes), (int)(y * tRes)] * -edgeHeight, x));
                tUV[x * 4 + h * 8 + 3 + w * 4] = Vector2.Scale(new Vector2(y * tRes, x * tRes), uvScale);
            }

            int index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                tPolys[index++] = y * 4;
                tPolys[index++] = y * 4 + 1;
                tPolys[index++] = y * 4 + 2;

                tPolys[index++] = y * 4;
                tPolys[index++] = y * 4 + 2;
                tPolys[index++] = y * 4 + 3;

                tPolys[index++] = y * 4 + h * 4;
                tPolys[index++] = y * 4 + 1 + h * 4;
                tPolys[index++] = y * 4 + 2 + h * 4;

                tPolys[index++] = y * 4 + h * 4;
                tPolys[index++] = y * 4 + 2 + h * 4;
                tPolys[index++] = y * 4 + 3 + h * 4;
            }

            for (int x = 0; x < h - 1; x++)
            {
                tPolys[index++] = x * 4 + h * 8;
                tPolys[index++] = x * 4 + 1 + h * 8;
                tPolys[index++] = x * 4 + 2 + h * 8;

                tPolys[index++] = x * 4 + h * 8;
                tPolys[index++] = x * 4 + 2 + h * 8;
                tPolys[index++] = x * 4 + 3 + h * 8;

                tPolys[index++] = x * 4 + h * 8 + w * 4;
                tPolys[index++] = x * 4 + 1 + h * 8 + w * 4;
                tPolys[index++] = x * 4 + 2 + h * 8 + w * 4;

                tPolys[index++] = x * 4 + h * 8 + w * 4;
                tPolys[index++] = x * 4 + 2 + h * 8 + w * 4;
                tPolys[index++] = x * 4 + 3 + h * 8 + w * 4;
            }


            Mesh mesh = new Mesh();
            mesh.vertices = tVertices;

            mesh.triangles = tPolys;
            mesh.uv = tUV;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }
}
