using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;
using Assets.Simulation.Visibility;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexCellShaderData : MonoBehaviour {

        #region static fields and properties

        private static readonly float TransitionSpeed = 255f;

        #endregion

        #region instance fields and properties

        private Texture2D CellTexture;
        private Color32[] CellTextureData;

        private List<IHexCell> TransitioningCells = new List<IHexCell>();



        private IGameCore           GameCore;
        private IVisibilityCanon    VisibilityCanon;
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IGameCore gameCore, IVisibilityCanon visibilityCanon,
            IHexMapRenderConfig renderConfig
        ) {
            GameCore        = gameCore;
            VisibilityCanon = visibilityCanon;
            RenderConfig    = renderConfig;
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

            if(CellTextureData == null || CellTextureData.Length != x * z) {
                CellTextureData = new Color32[x * z];
            }else {
                for(int i = 0; i < CellTextureData.Length; i++) {
                    CellTextureData[i] = new Color32(0, 0, 0, 0);
                }
            }

            TransitioningCells.Clear();
        }

        public void RefreshTerrain(IHexCell cell) {
            if(cell.Terrain.IsWater()) {
                CellTextureData[cell.Index].a = (byte)RenderConfig.WaterTerrainIndex;
            }else if(cell.Shape == CellShape.Mountains) {
                CellTextureData[cell.Index].a = (byte)RenderConfig.MountainTerrainIndex;
            }else if(cell.Terrain == CellTerrain.FloodPlains) {
                CellTextureData[cell.Index].a = (byte)CellTerrain.Desert;
            }else {
                CellTextureData[cell.Index].a = (byte)cell.Terrain;
            }

            enabled = true;
        }

        public void RefreshVisibility(IHexCell cell) {
            if(VisibilityCanon.RevealMode == RevealMode.Immediate) {
                if(GameCore.ActiveCivilization != null) {
                    CellTextureData[cell.Index].r = 
                        VisibilityCanon.IsCellVisible(cell) ? (byte)255 : (byte)0;
                }else if(CellTextureData[cell.Index].b != 255) {
                    CellTextureData[cell.Index].b = 255;
                    CellTextureData[cell.Index].r = 0;
                }
            }else if(VisibilityCanon.RevealMode == RevealMode.Fade) {
                TransitioningCells.Add(cell);
            }else {
                throw new NotImplementedException("No behavior specified for RevealMode " + VisibilityCanon.RevealMode);
            }
            
            enabled = true;
        }

        public void SetMapData(IHexCell cell, float data) {
            CellTextureData[cell.Index].b = data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 254f) : (byte)254f);
        }

        private bool UpdateCellData(IHexCell cell, int delta) {
            int index = cell.Index;
            Color32 data = CellTextureData[index];
            bool stillUpdating = false;

            var activeCiv = GameCore.ActiveCivilization;

            if(VisibilityCanon.IsCellVisible(cell) && data.r < 255) {
                stillUpdating = true;
                int newVisibility = data.r + delta;
                data.r = newVisibility >= 255 ? (byte)255 : (byte)newVisibility;

            }else if(data.r > 0) {
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
