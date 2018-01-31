using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;

namespace Assets.Simulation.HexMap {

    public class HexCellShaderData : MonoBehaviour {

        #region instance fields and properties

        private Texture2D CellTexture;
        private Color32[] CellTextureData;



        private IGameCore            GameCore;
        private ICellVisibilityCanon VisibilityCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IGameCore gameCore, ICellVisibilityCanon visibilityCanon) {
            GameCore        = gameCore;
            VisibilityCanon = visibilityCanon;
        }

        #region Unity messages

        private void LateUpdate() {
            CellTexture.SetPixels32(CellTextureData);
            CellTexture.Apply();
            enabled = false;
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
            
            Shader.SetGlobalVector(
                "_HexCellData_TexelSize",
                new Vector4(1f / x, 1f / z, x, z)
            );

            if(CellTextureData == null || CellTextureData.Length != x * z) {
                CellTextureData = new Color32[x * z];
            }else {
                for(int i = 0; i < CellTextureData.Length; i++) {
                    CellTextureData[i] = new Color32(0, 0, 0, 0);
                }
            }
        }

        public void RefreshTerrain(IHexCell cell) {
            CellTextureData[cell.Index].a = (byte)cell.Terrain;
            enabled = true;
        }

        public void RefreshVisibility(IHexCell cell) {
            CellTextureData[cell.Index].r = 
                VisibilityCanon.IsCellVisibleToCiv(cell, GameCore.ActiveCivilization) ? (byte)255 : (byte)0;
            enabled = true;
        }

        #endregion

    }

}
