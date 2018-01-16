using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.UI;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities;

namespace Assets.UI.HexMap {

    public class TileHoverDisplay : MonoBehaviour {

        #region instance fields and properties

        [InjectOptional(Id = "Terrain Type Field")]
        private Text TerrainTypeField {
            get { return _terrainTypeField; }
            set {
                if(value != null) {
                    _terrainTypeField = value;
                }
            }
        }
        [SerializeField] private Text _terrainTypeField;

        [InjectOptional(Id = "Terrain Shape Field")]
        private Text TerrainShapeField {
            get { return _terrainShapeField; }
            set {
                if(value != null) {
                    _terrainShapeField = value;
                }
            }
        }
        [SerializeField] private Text _terrainShapeField;

        [InjectOptional(Id = "Terrain Feature Field")]
        private Text TerrainFeatureField {
            get { return _terrainFeatureField; }
            set {
                if(value != null) {
                    _terrainFeatureField = value;
                }
            }
        }
        [SerializeField] private Text _terrainFeatureField;

        private IResourceSummaryDisplay YieldDisplay;

        private IResourceGenerationLogic GenerationLogic;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Tile Hover Yield Display")] IResourceSummaryDisplay yieldDisplay,
            IHexCellSignalLogic signalLogic, IResourceGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon
        ){
            YieldDisplay = yieldDisplay;

            signalLogic.BeginHoverSignal.Subscribe(OnBeginHoverFired);
            signalLogic.EndHoverSignal.Subscribe(OnEndHoverFired);

            GenerationLogic = generationLogic;
            CellPossessionCanon = tilePossessionCanon;
        }

        private void OnBeginHoverFired(IHexCell hoveredTile) {
            TerrainTypeField.text = hoveredTile.Terrain.ToString();
            TerrainShapeField.text = hoveredTile.Shape.ToString();
            TerrainFeatureField.text = hoveredTile.Feature.ToString();

            var cellOwner = CellPossessionCanon.GetOwnerOfPossession(hoveredTile);

            if(cellOwner != null) {
                YieldDisplay.DisplaySummary(GenerationLogic.GetYieldOfSlotForCity(hoveredTile.WorkerSlot, cellOwner));
            }else {
                YieldDisplay.DisplaySummary(hoveredTile.WorkerSlot.BaseYield);
            }
            
            transform.position = Camera.main.WorldToScreenPoint(hoveredTile.transform.position);

            gameObject.SetActive(true);            
        }

        private void OnEndHoverFired(IHexCell unhoveredTile) {
            gameObject.SetActive(false);
        }

        #endregion

    }

}
