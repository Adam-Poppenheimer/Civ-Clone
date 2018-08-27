using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class CellClimateLogic : ICellClimateLogic {

        #region instance fields and properties

        private Dictionary<IHexCell, float> TemperatureOfCell   = new Dictionary<IHexCell, float>();
        private Dictionary<IHexCell, float> PrecipitationOfCell = new Dictionary<IHexCell, float>();

        private Rect TextureSampleRegion;

        private bool HasSetSampleRegion = false;

        private Texture2D PrecipitationTexture;



        private IHexGrid             Grid;
        private IMapGenerationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public CellClimateLogic(
            IHexGrid grid, IMapGenerationConfig config
        ) {
            Grid   = grid;
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ICellTemperatureLogic

        public float GetTemperatureOfCell(IHexCell cell) {
            float retval;

            if(!TemperatureOfCell.TryGetValue(cell, out retval)) {
                retval = CalculateTemperatureOfCell(cell);

                TemperatureOfCell[cell] = retval;                
            }

            return retval;
        }

        public float GetPrecipitationOfCell(IHexCell cell) {
            float retval;

            if(!PrecipitationOfCell.TryGetValue(cell, out retval)) {
                retval = CalculatePrecipitationOfCell(cell);

                PrecipitationOfCell[cell] = retval;
            }

            return retval;
        }

        public void Reset(IMapTemplate newTemplate) {
            TemperatureOfCell  .Clear();
            PrecipitationOfCell.Clear();

            HasSetSampleRegion = false;

            PrecipitationTexture = newTemplate.PrecipitationTexture;
        }

        #endregion

        private float CalculateTemperatureOfCell(IHexCell cell) {
            float latitude = (float)cell.Coordinates.Z / Grid.CellCountZ;
            if(Config.Hemispheres == HemisphereMode.Both) {
                latitude *= 2;
                if(latitude > 1f) {
                    latitude = 2f - latitude;
                }
            }else if(Config.Hemispheres == HemisphereMode.North) {
                latitude = 1f - latitude;
            }

            return Mathf.LerpUnclamped(Config.LowTemperature, Config.HighTemperature, latitude);
        }

        private float CalculatePrecipitationOfCell(IHexCell cell) {
            if(!HasSetSampleRegion) {
                SetSampleRegion();
            }

            int cellOffsetX = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            int cellOffsetZ = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            float cellNormalX = (float)cellOffsetX / Grid.CellCountX;
            float cellNormalZ = (float)cellOffsetZ / Grid.CellCountZ;

            Color sample = PrecipitationTexture.GetPixelBilinear(
                TextureSampleRegion.x + cellNormalX * TextureSampleRegion.width,
                TextureSampleRegion.y + cellNormalZ * TextureSampleRegion.height
            );

            return sample.r;
        }

        //Currently subdivides the texture into 16 square regions
        private void SetSampleRegion() {
            float uStart = UnityEngine.Random.Range(0, 3) * 0.25f;
            float vStart = UnityEngine.Random.Range(0, 3) * 0.25f;

            TextureSampleRegion = new Rect(uStart, vStart, 0.25f, 0.25f);

            HasSetSampleRegion = true;
        }

        #endregion

    }

}
