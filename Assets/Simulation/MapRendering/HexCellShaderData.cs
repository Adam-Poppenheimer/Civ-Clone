using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;
using Assets.Simulation.Visibility;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class HexCellShaderData : MonoBehaviour, IHexCellShaderData {

        #region static fields and properties

        private static readonly float TransitionSpeed = 255f;

        #endregion

        #region instance fields and properties

        private Texture2D CellTexture;
        private Color32[] CellTextureData;

        private List<IHexCell> TransitioningCells = new List<IHexCell>();



        
        private IVisibilityCanon  VisibilityCanon;
        private IExplorationCanon ExplorationCanon;
        private IMapRenderConfig  RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon, IMapRenderConfig renderConfig
        ) {
            VisibilityCanon          = visibilityCanon;
            ExplorationCanon         = explorationCanon;
            RenderConfig             = renderConfig;
        }

        #region Unity messages

        private void LateUpdate() {
            int delta = (int)(Time.deltaTime * TransitionSpeed);
            if(delta <= 0) {
                delta = 1;
            }

            for(int i = 0; i < TransitioningCells.Count; i++) {
                if(!UpdateCellData(TransitioningCells[i], delta)) {
                    TransitioningCells[i--] = TransitioningCells[TransitioningCells.Count - 1];
                    TransitioningCells.RemoveAt(TransitioningCells.Count - 1);
                }
            }

            CellTexture.SetPixels32(CellTextureData);
            CellTexture.Apply();
            enabled = TransitioningCells.Count > 0;
        }

        #endregion

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

            TransitioningCells.Clear();

            enabled = true;
        }

        public void RefreshVisibility(IHexCell cell) {
            if(VisibilityCanon.RevealMode == RevealMode.Immediate) {
                CellTextureData[cell.Index].r = VisibilityCanon.IsCellVisible(cell) ? (byte)255 : (byte)0;

                if(CellTextureData[cell.Index].b != 255) {
                    CellTextureData[cell.Index].b = 255;
                    CellTextureData[cell.Index].r = 0;
                }
            }else if(VisibilityCanon.RevealMode == RevealMode.Fade) {
                TransitioningCells.Add(cell);
            }else {
                throw new NotImplementedException("No behavior specified for RevealMode " + VisibilityCanon.RevealMode);
            }

            CellTextureData[cell.Index].g = ExplorationCanon.IsCellExplored(cell) ? (byte)255 : (byte)0;
            
            enabled = true;
        }

        public void SetMapData(IHexCell cell, float data) {
            CellTextureData[cell.Index].b = data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 254f) : (byte)254f);
        }

        private bool UpdateCellData(IHexCell cell, int delta) {
            int index = cell.Index;
            Color32 data = CellTextureData[index];
            bool stillUpdating = false;

            if(VisibilityCanon.IsCellVisible(cell) && data.r < 255) {
                stillUpdating = true;
                int newVisibility = data.r + delta;
                data.r = newVisibility >= 255 ? (byte)255 : (byte)newVisibility;

            }else if(!VisibilityCanon.IsCellVisible(cell) && data.r > 0) {
                stillUpdating = true;
                int newVisibility = data.r - delta;
                data.r = newVisibility < 0 ? (byte)0 : (byte)newVisibility;
            }

            if(!stillUpdating) {
                data.b = 0;
            }

            CellTextureData[index] = data;
            return stillUpdating;
        }

        #endregion

    }

}
