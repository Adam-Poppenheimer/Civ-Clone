using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;
using UniRx;

namespace Assets.Simulation.MapRendering {

    public class TerrainBaker : MonoBehaviour, ITerrainBaker {

        #region instance fields and properties

        [SerializeField] private LayerMask OcclusionMask    = 0;
        [SerializeField] private LayerMask LandDrawingMask  = 0;
        [SerializeField] private LayerMask WaterDrawingMask = 0;

        [SerializeField] private TerrainSubBaker SubBakerPrefab = null;

        private Queue<Tuple<TerrainSubBaker, TerrainSubBaker>> FreeBakerPairs =
            new Queue<Tuple<TerrainSubBaker, TerrainSubBaker>>();

        private HashSet<IMapChunk> UnbakedChunks = new HashSet<IMapChunk>();

        private Dictionary<IMapChunk, Tuple<TerrainSubBaker, TerrainSubBaker>> PairsOfUnreadChunks =
            new Dictionary<IMapChunk, Tuple<TerrainSubBaker, TerrainSubBaker>>();




        private IMapRenderConfig    RenderConfig;
        private IBakedElementsLogic BakedElementsLogic;
        private ITerrainBakeConfig  TerrainBakeConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IMapRenderConfig renderConfig, IBakedElementsLogic bakedElementsLogic,
            ITerrainBakeConfig terrainBakeConfig, DiContainer container
        ) {
            RenderConfig       = renderConfig;
            BakedElementsLogic = bakedElementsLogic;
            TerrainBakeConfig  = terrainBakeConfig;

            for(int i = 0; i < RenderConfig.MaxParallelTerrainRefreshes; i++) {
                FreeBakerPairs.Enqueue(new Tuple<TerrainSubBaker, TerrainSubBaker>(
                    container.InstantiatePrefabForComponent<TerrainSubBaker>(SubBakerPrefab, transform),
                    container.InstantiatePrefabForComponent<TerrainSubBaker>(SubBakerPrefab, transform)
                ));
            }

            int highResWidth  = Mathf.RoundToInt(TerrainBakeConfig.BakeTextureDimensionsHighRes.x);
            int highResHeight = Mathf.RoundToInt(TerrainBakeConfig.BakeTextureDimensionsHighRes.y);
        }

        #region Unity messages

        private void Update() {
            if(UnbakedChunks.Count > 0) {
                TryBakeChunks();
            }

            if(PairsOfUnreadChunks.Count > 0) {
                TryFreeFinishedBakers();
            }
        }

        #endregion

        public void BakeIntoChunk(IMapChunk chunk) {
            UnbakedChunks.Add(chunk);
        }

        private void TryBakeChunks() {
            Profiler.BeginSample("TryBakeChunks()");

            int dataCount = Math.Min(RenderConfig.MaxParallelTerrainRefreshes, FreeBakerPairs.Count);

            var chunksToProcess = UnbakedChunks.Where(chunk => !chunk.IsRefreshing).Take(dataCount).ToArray();

            for(int i = 0; i < chunksToProcess.Length; i++) {
                IMapChunk activeChunk = chunksToProcess[i];
                Texture2D newLand, newWater;

                GetNewTextures(activeChunk, out newLand, out newWater);

                UnbakedChunks.Remove(activeChunk);

                if(newLand.width > 0 && newLand.height > 0 && newWater.width > 0 && newWater.height > 0) {
                    var servingBakerPair = FreeBakerPairs.Dequeue();

                    BakeLand (activeChunk, newLand,  servingBakerPair.Item1);
                    BakeWater(activeChunk, newWater, servingBakerPair.Item2);

                    servingBakerPair.Item1.ReadPixels(newLand, texture => {
                        if(activeChunk.LandBakeTexture != null) {
                            Destroy(activeChunk.LandBakeTexture);
                        }

                        activeChunk.LandBakeTexture = texture;
                    });

                    servingBakerPair.Item2.ReadPixels(newWater, texture => {
                        if(activeChunk.WaterBakeTexture != null) {
                            Destroy(activeChunk.WaterBakeTexture);
                        }

                        activeChunk.WaterBakeTexture = texture;
                    });
                
                    PairsOfUnreadChunks[activeChunk] = servingBakerPair;
                }else {
                    activeChunk.LandBakeTexture  = newLand;
                    activeChunk.WaterBakeTexture = newWater;
                }
            }

            Profiler.EndSample();
        }

        private void BakeLand(IMapChunk chunk, Texture2D landTexture, TerrainSubBaker subBaker) {
            var landLayer  = chunk.Terrain.gameObject.layer;

            chunk.Terrain.gameObject.layer = LayerMask.NameToLayer("Terrain Bake Temp");

            subBaker.PerformBakePass(
                chunk, landTexture, CameraClearFlags.SolidColor, OcclusionMask,
                TerrainBakeConfig.TerrainBakeOcclusionShader, "RenderType"
            );

            subBaker.PerformBakePass(
                chunk, landTexture, CameraClearFlags.Nothing, LandDrawingMask
            );

            chunk.Terrain.gameObject.layer = landLayer;
        }

        private void BakeWater(IMapChunk chunk, Texture2D waterTexture, TerrainSubBaker subBaker) {
            subBaker.PerformBakePass(
                chunk, waterTexture, CameraClearFlags.SolidColor, WaterDrawingMask
            );
        }

        private void TryFreeFinishedBakers() {
            var finishedPairs = PairsOfUnreadChunks.Where(keyValue => keyValue.Value.Item1.IsReady && keyValue.Value.Item2.IsReady)
                                                   .ToArray();

            foreach(var chunkBakerPairs in finishedPairs) {
                PairsOfUnreadChunks.Remove(chunkBakerPairs.Key);

                FreeBakerPairs.Enqueue(chunkBakerPairs.Value);
            }
        }


        private void GetNewTextures(IMapChunk chunk, out Texture2D newLand, out Texture2D newWater) {
            BakedElementFlags bakedElements = BakedElementsLogic.GetBakedElementsInCells(chunk.CenteredCells);
            bakedElements |= BakedElementsLogic.GetBakedElementsInCells(chunk.OverlappingCells);

            Vector2 textureSize = GetBakeTextureDimensionsForElements(bakedElements);

            int textureX = Mathf.RoundToInt(textureSize.x);
            int textureY = Mathf.RoundToInt(textureSize.y);

            newLand = new Texture2D(textureX, textureY, TextureFormat.ARGB32, false);

            newLand.filterMode = FilterMode.Point;
            newLand.wrapMode   = TextureWrapMode.Clamp;
            newLand.anisoLevel = 0;

            newLand.name = "Land Bake Texture";

            newWater = new Texture2D(textureX, textureY, TextureFormat.ARGB32, false);

            newWater.filterMode = FilterMode.Point;
            newWater.wrapMode   = TextureWrapMode.Clamp;
            newWater.anisoLevel = 0;

            newWater.name = "Water Bake Texture";
        }

        private Vector2 GetBakeTextureDimensionsForElements(BakedElementFlags elements) {
            if((elements & TerrainBakeConfig.BakeElementsHighRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsHighRes;

            }else if((elements & TerrainBakeConfig.BakeElementsMediumRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsMediumRes;

            }else if((elements & TerrainBakeConfig.BakeElementsLowRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsLowRes;

            }else {
                return Vector2.zero;
            }
        }

        #endregion

    }

}
