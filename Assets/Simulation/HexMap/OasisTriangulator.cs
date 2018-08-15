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

        #endregion

        #region constructors

        [Inject]
        public OasisTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IOasisTriangulator

        public bool ShouldTriangulateOasis(CellTriangulationData data) {
            return data.Center.Feature == CellFeature.Oasis;
        }

        public void TriangulateOasis(CellTriangulationData data) {
            Vector3 perturbedPeak = NoiseGenerator.Perturb(data.CenterPeak);

            Vector3 nearIntermediateV1 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V1, HexMetrics.OasisWaterLerp);
            Vector3 nearIntermediateV3 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V3, HexMetrics.OasisWaterLerp);
            Vector3 nearIntermediateV5 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V5, HexMetrics.OasisWaterLerp);

            Vector3 farIntermediateV1 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V1, HexMetrics.OasisVerdantEdgeLerp);
            Vector3 farIntermediateV3 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V3, HexMetrics.OasisVerdantEdgeLerp);
            Vector3 farIntermediateV5 = Vector3.Lerp(perturbedPeak, data.CenterToRightEdgePerturbed.V5, HexMetrics.OasisVerdantEdgeLerp);

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
