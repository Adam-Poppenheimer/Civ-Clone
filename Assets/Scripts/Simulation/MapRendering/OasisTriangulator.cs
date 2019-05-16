using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class OasisTriangulator : IOasisTriangulator {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private INoiseGenerator  NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public OasisTriangulator(IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator) {
            RenderConfig   = renderConfig;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        public void TrianglateOasis(IHexCell cell, IHexMesh waterMesh, IHexMesh landMesh) {
            if(cell.Feature != CellFeature.Oasis) {
                return;
            }

            float waterDistance = RenderConfig.OasisWaterRadius;
            float landDistance  = RenderConfig.OasisWaterRadius + RenderConfig.OasisLandWidth;

            float tDelta = 2 * Mathf.PI / RenderConfig.OasisBoundarySegments;

            Vector3 perturbedCenter = NoiseGenerator.Perturb(cell.AbsolutePosition);

            Vector3 waterTwo = NoiseGenerator.Perturb(
                cell.AbsolutePosition + new Vector3(waterDistance, 0f, 0f)
            );

            Vector3 landTwo = NoiseGenerator.Perturb(
                cell.AbsolutePosition + new Vector3(landDistance, 0f, 0f)
            );

            for(float t = 0f; t < 2 * Mathf.PI; t += tDelta) {
                Vector3 waterOne = waterTwo;
                Vector3 landOne  = landTwo;

                waterTwo = NoiseGenerator.Perturb(
                    cell.AbsolutePosition + new Vector3(waterDistance * Mathf.Cos(t), 0f, waterDistance * Mathf.Sin(t))
                );

                landTwo = NoiseGenerator.Perturb(
                    cell.AbsolutePosition + new Vector3(landDistance * Mathf.Cos(t), 0f, landDistance * Mathf.Sin(t))
                );

                waterMesh.AddTriangle(perturbedCenter, waterTwo, waterOne);

                waterMesh.AddTriangleUV(Vector2.one, Vector2.one, Vector2.one);

                landMesh.AddQuad(waterTwo, waterOne, landTwo, landOne);
            }

            waterMesh.AddTriangle(
                perturbedCenter, NoiseGenerator.Perturb(cell.AbsolutePosition + new Vector3(waterDistance, 0f, 0f)), waterTwo
            );

            landMesh.AddQuad(
                NoiseGenerator.Perturb(cell.AbsolutePosition + new Vector3(waterDistance, 0f, 0f)), waterTwo,
                NoiseGenerator.Perturb(cell.AbsolutePosition + new Vector3(landDistance,  0f, 0f)), landTwo
            );

            waterMesh.AddTriangleUV(Vector2.one, Vector2.one, Vector2.one);
        }

        #endregion

    }

}
