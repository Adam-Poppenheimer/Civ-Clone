using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class OasisTriangulator : IOasisTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public OasisTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IHexMapRenderConfig renderConfig
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
            RenderConfig   = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IOasisTriangulator

        public bool ShouldTriangulateOasis(CellTriangulationData data) {
            return data.Center.Feature == CellFeature.Oasis;
        }

        public void TriangulateOasis(CellTriangulationData data) {
            Vector3 perturbedPeak = NoiseGenerator.Perturb(data.CenterPeak);

            Vector3 nearIntermediateV1 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V1, RenderConfig.OasisWaterLerp);
            Vector3 nearIntermediateV3 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V3, RenderConfig.OasisWaterLerp);
            Vector3 nearIntermediateV5 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V5, RenderConfig.OasisWaterLerp);

            Vector3 farIntermediateV1 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V1, RenderConfig.OasisVerdantEdgeLerp);
            Vector3 farIntermediateV3 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V3, RenderConfig.OasisVerdantEdgeLerp);
            Vector3 farIntermediateV5 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V5, RenderConfig.OasisVerdantEdgeLerp);

            Vector3 indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            MeshBuilder.AddTriangleUnperturbed(
                perturbedPeak, MeshBuilder.Weights1,
                nearIntermediateV1, MeshBuilder.Weights1,
                nearIntermediateV3, MeshBuilder.Weights1,
                indices, MeshBuilder.Oases
            );

            MeshBuilder.AddTriangleUnperturbed(
                perturbedPeak, MeshBuilder.Weights1,
                nearIntermediateV3, MeshBuilder.Weights1,
                nearIntermediateV5, MeshBuilder.Weights1,
                indices, MeshBuilder.Oases
            );

            MeshBuilder.AddQuadUnperturbed(
                nearIntermediateV1, MeshBuilder.Weights1, new Vector2(0f, 1.5f),
                nearIntermediateV3, MeshBuilder.Weights1, new Vector2(0f, 1.5f),
                farIntermediateV1,  MeshBuilder.Weights1, new Vector2(0f, 0f),
                farIntermediateV3,  MeshBuilder.Weights1, new Vector2(0f, 0f),
                indices, MeshBuilder.FloodPlains
            );

            MeshBuilder.AddQuadUnperturbed(
                nearIntermediateV3, MeshBuilder.Weights1, new Vector2(0f, 1.5f),
                nearIntermediateV5, MeshBuilder.Weights1, new Vector2(0f, 1.5f),
                farIntermediateV3,  MeshBuilder.Weights1, new Vector2(0f, 0f),
                farIntermediateV5,  MeshBuilder.Weights1, new Vector2(0f, 0f),
                indices, MeshBuilder.FloodPlains
            );
        }

        #endregion

        #endregion
        
    }

}
