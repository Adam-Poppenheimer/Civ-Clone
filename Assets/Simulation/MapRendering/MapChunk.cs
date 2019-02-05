using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class MapChunk : MonoBehaviour, IMapChunk {

        #region instance fields and properties

        private Terrain Terrain;

        private Coroutine RefreshAlphamapCoroutine;



        private ITerrainAlphamapLogic AlphamapLogic;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            ITerrainAlphamapLogic alphamapLogic, IMapRenderConfig renderConfig
        ) {
            AlphamapLogic = alphamapLogic;
            RenderConfig  = renderConfig;
        }

        #region from IMapChunk

        public void InitializeTerrain(Vector3 position, float width, float height) {
            var terrainData = BuildTerrainData(width, height);

            var terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);

            terrainGameObject.transform.SetParent(transform, false);

            Terrain = terrainGameObject.GetComponent<Terrain>();

            transform.position = position;

            Terrain.Flush();
        }

        public void RefreshAlphamap() {
            if(RefreshAlphamapCoroutine == null) {
                RefreshAlphamapCoroutine = StartCoroutine(RefreshAlphamap_Perform());
            }
        }

        public void RefreshAll() {
            RefreshAlphamap();
        }

        public bool DoesCellOverlapChunk(IHexCell cell) {
            return RenderConfig.Corners.Any(corner => IsOnTerrain2D(corner + cell.AbsolutePosition));
        }

        public bool IsOnTerrain2D(Vector3 position3D) {
            float xMin = transform.position.x;
            float xMax = transform.position.x + Terrain.terrainData.size.x;
            float zMin = transform.position.z;
            float zMax = transform.position.z + Terrain.terrainData.size.z;

            return position3D.x >= xMin && position3D.x <= xMax
                && position3D.z >= zMin && position3D.z <= zMax;
        }

        #endregion

        private IEnumerator RefreshAlphamap_Perform() {
            yield return new WaitForEndOfFrame();

            var terrainData = Terrain.terrainData;

            int mapWidth  = terrainData.alphamapWidth;
            int mapHeight = terrainData.alphamapHeight;

            float widthToWorldX  = terrainData.size.x / mapWidth;
            float heightToWorldZ = terrainData.size.z / mapHeight;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

            for(int widthIndex = 0; widthIndex < mapWidth; widthIndex++) {
                for(int heightIndex = 0; heightIndex < mapHeight; heightIndex++) {

                    Vector3 worldPosition = transform.position + new Vector3(
                        widthIndex * widthToWorldX, 0f, heightIndex * heightToWorldZ
                    );

                    float[] newAlphas = AlphamapLogic.GetAlphamapForPosition(worldPosition);

                    for(int alphaIndex = 0; alphaIndex < terrainData.alphamapTextures.Length; alphaIndex++) {
                        alphaMaps[widthIndex, heightIndex, alphaIndex] = newAlphas[alphaIndex];
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);

            Terrain.Flush();
        }

        private TerrainData BuildTerrainData(float width, float height) {
            var newData = new TerrainData();

            newData.alphamapResolution  = RenderConfig.TerrainAlphamapResolution;
            newData.heightmapResolution = RenderConfig.TerrainHeightmapResolution;

            newData.size = new Vector3(width, RenderConfig.MaxElevation, height);

            newData.splatPrototypes = RenderConfig.MapTextures.Select(
                mapTexture => new SplatPrototype() {
                    texture = mapTexture
                }
            ).ToArray();

            return newData;
        }

        #endregion
        
    }

}
