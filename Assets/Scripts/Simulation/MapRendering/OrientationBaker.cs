using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class OrientationBaker : MonoBehaviour, IOrientationBaker {

        #region instance fields and properties

        [SerializeField] private LayerMask OrientationCullingMask   = 0;
        [SerializeField] private LayerMask NormalWeightsCullingMask = 0;
        [SerializeField] private LayerMask RiverWeightsCullingMask  = 0;
        [SerializeField] private LayerMask RiverDuckCullingMask     = 0;

        [SerializeField] private OrientationSubBaker SubBakerPrefab = null;

        private IHexMesh OrientationMesh {
            get {
                if(orientationMesh == null) {
                    orientationMesh = HexMeshFactory.Create("Orientation Mesh", RenderConfig.OrientationMeshData);

                    orientationMesh.transform.SetParent(transform, false);
                }

                return orientationMesh;
            }
        }
        private IHexMesh orientationMesh;

        private IHexMesh WeightsMesh {
            get {
                if(weightsMesh == null) {
                    weightsMesh = HexMeshFactory.Create("Weights Mesh", RenderConfig.WeightsMeshData);

                    weightsMesh.transform.SetParent(transform, false);
                }

                return weightsMesh;
            }
        }
        private IHexMesh weightsMesh;

        private Queue<Tuple<OrientationSubBaker, OrientationSubBaker, OrientationSubBaker>> FreeBakerTriplets =
            new Queue<Tuple<OrientationSubBaker, OrientationSubBaker, OrientationSubBaker>>();

        private HashSet<ChunkOrientationData> UnrenderedOrientationData = new HashSet<ChunkOrientationData>();
        



        private IMapRenderConfig         RenderConfig;
        private IHexMeshFactory          HexMeshFactory;
        private IOrientationTriangulator OrientationTriangulator;
        private IWeightsTriangulator     WeightsTriangulator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapRenderConfig renderConfig, IHexMeshFactory hexMeshFactory,
            IOrientationTriangulator orientationTriangulator, IWeightsTriangulator weightsTriangulator,
            DiContainer container
        ) {
            RenderConfig            = renderConfig;
            HexMeshFactory          = hexMeshFactory;
            OrientationTriangulator = orientationTriangulator;
            WeightsTriangulator     = weightsTriangulator;

            for(int i = 0; i < RenderConfig.MaxParallelTerrainRefreshes; i++) {
                FreeBakerTriplets.Enqueue(new Tuple<OrientationSubBaker, OrientationSubBaker, OrientationSubBaker>(
                    container.InstantiatePrefabForComponent<OrientationSubBaker>(SubBakerPrefab, transform),
                    container.InstantiatePrefabForComponent<OrientationSubBaker>(SubBakerPrefab, transform),
                    container.InstantiatePrefabForComponent<OrientationSubBaker>(SubBakerPrefab, transform)
                ));
            }
        }

        #region Unity messages

        private void OnDestroy() {
            foreach(var orientationData in UnrenderedOrientationData) {
                ReleaseOrientationData(orientationData);
            }

            if(orientationMesh != null) {
                orientationMesh.Clear();
                HexMeshFactory.Destroy(orientationMesh);
            }

            if(weightsMesh != null) {
                weightsMesh.Clear();
                HexMeshFactory.Destroy(weightsMesh);
            }
        }

        private void Update() {
            if(UnrenderedOrientationData.Count > 0) {
                TryRenderOrientationData();
            }            
        }

        #endregion

        #region from IOrientationBaker

        public ChunkOrientationData MakeOrientationRequestForChunk(IMapChunk chunk) {
            var newData = new ChunkOrientationData(chunk);

            UnrenderedOrientationData.Add(newData);

            return newData;
        }

        public void ReleaseOrientationData(ChunkOrientationData data) {
            FreeBakerTriplets.Enqueue(data.BakerTriplet);

            data.BakerTriplet = null;
        }

        #endregion

        private void TryRenderOrientationData() {
            int dataCount = Math.Min(RenderConfig.MaxParallelTerrainRefreshes, FreeBakerTriplets.Count);

            var dataToProcess = UnrenderedOrientationData.Take(dataCount).ToArray();

            for(int i = 0; i < dataToProcess.Length; i++) {
                var activeData = dataToProcess[i];

                activeData.BakerTriplet = FreeBakerTriplets.Dequeue();

                RenderOrientation(activeData.Chunk, activeData.BakerTriplet.Item1);
                RenderWeights    (activeData.Chunk, activeData.BakerTriplet.Item2);
                RenderDuck       (activeData.Chunk, activeData.BakerTriplet.Item3);
            }

            for(int i = 0; i < dataToProcess.Length; i++) {
                var activeData = dataToProcess[i];

                activeData.BakerTriplet.Item1.ReadPixels();
                activeData.BakerTriplet.Item2.ReadPixels();
                activeData.BakerTriplet.Item3.ReadPixels();

                UnrenderedOrientationData.Remove(activeData);
            }
        }

        private void RenderOrientation(IMapChunk chunk, OrientationSubBaker subBaker) {
            OrientationMesh.Clear();

            OrientationMesh.transform.SetParent(chunk.transform, false);

            foreach(var cell in chunk.CenteredCells) {
                OrientationTriangulator.TriangulateOrientation(cell, OrientationMesh);
            }

            foreach(var cell in chunk.OverlappingCells) {
                OrientationTriangulator.TriangulateOrientation(cell, OrientationMesh);
            }

            OrientationMesh.Apply();

            subBaker.PerformBakePass(chunk, CameraClearFlags.SolidColor, OrientationCullingMask);
        }

        private void RenderWeights(IMapChunk chunk, OrientationSubBaker subBaker) {
            WeightsMesh.Clear();

            WeightsMesh.transform.SetParent(chunk.transform, false);

            foreach(var cell in chunk.CenteredCells) {
                WeightsTriangulator.TriangulateCellWeights(cell, WeightsMesh);
            }

            foreach(var cell in chunk.OverlappingCells) {
                WeightsTriangulator.TriangulateCellWeights(cell, WeightsMesh);
            }

            WeightsMesh.Apply();

            subBaker.PerformBakePass(chunk, CameraClearFlags.SolidColor, NormalWeightsCullingMask);
            subBaker.PerformBakePass(chunk, CameraClearFlags.Nothing,    RiverWeightsCullingMask, RenderConfig.RiverWeightShader, "RenderType");
        }

        private void RenderDuck(IMapChunk chunk, OrientationSubBaker subBaker) {
            subBaker.PerformBakePass(chunk, CameraClearFlags.SolidColor, RiverDuckCullingMask);
        }

        #endregion

    }

}
