using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.Core;
using Assets.Simulation.Visibility;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class HexCellShaderData : IHexCellShaderData {

        #region static fields and properties

        private static readonly float TransitionSpeed = 255f;

        #endregion

        #region instance fields and properties

        private Texture2D CellTexture;
        private Color32[] CellTextureData;

        private Coroutine ApplyTextureCoroutine;



        
        private IVisibilityCanon  VisibilityCanon;
        private IExplorationCanon ExplorationCanon;
        private IMapRenderConfig  RenderConfig;
        private MonoBehaviour     CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public HexCellShaderData(
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon, IMapRenderConfig renderConfig,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            VisibilityCanon  = visibilityCanon;
            ExplorationCanon = explorationCanon;
            RenderConfig     = renderConfig;
            CoroutineInvoker = coroutineInvoker;
        }

        #endregion

        #region instance methods

        #region from IHexCellShaderData

        public void Initialize(int x, int z) {
            if(CellTexture != null) {
                CellTexture.Resize(x, z);
            }else {
                CellTexture = new Texture2D(
                    x, z, TextureFormat.RGBA32, false, true
                );

                CellTexture.filterMode = FilterMode.Point;
                CellTexture.wrapMode   = TextureWrapMode.Clamp;
                Shader.SetGlobalTexture("_HexCellData", CellTexture);
            }
            
            Shader.SetGlobalVector("_HexCellData_TexelSize", new Vector4(1f / x, 1f / z, x, z));

            Shader.SetGlobalFloat("_Hex_InnerRadiusTimesTwo",   RenderConfig.InnerRadius * 2f);
            Shader.SetGlobalFloat("_Hex_OuterRadiusTimesThree", RenderConfig.OuterRadius * 3f);

            if(CellTextureData == null || CellTextureData.Length != x * z) {
                CellTextureData = new Color32[x * z];
            }else {
                for(int i = 0; i < CellTextureData.Length; i++) {
                    CellTextureData[i] = new Color32(0, 0, 0, 0);
                }
            }
        }

        public void RefreshVisibility(IHexCell cell) {
            CellTextureData[cell.Index].r = VisibilityCanon .IsCellVisible (cell) ? (byte)255 : (byte)0;
            CellTextureData[cell.Index].g = ExplorationCanon.IsCellExplored(cell) ? (byte)255 : (byte)0;
            CellTextureData[cell.Index].b = VisibilityCanon .IsCellVisible (cell) ? (byte)255 : (byte)0;

            if(ApplyTextureCoroutine == null) {
                ApplyTextureCoroutine = CoroutineInvoker.StartCoroutine(ApplyTexture());
            }
        }

        public void SetMapData(IHexCell cell, float data) {
            CellTextureData[cell.Index].b = data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 254f) : (byte)254f);
        }

        #endregion

        private IEnumerator ApplyTexture() {
            yield return new WaitForEndOfFrame();

            CellTexture.SetPixels32(CellTextureData);
            CellTexture.Apply();

            ApplyTextureCoroutine = null;
        }

        #endregion

    }

}
