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

        [SerializeField] private Text TerrainTypeField;

        [SerializeField] private Text TerrainFeatureField;

        [SerializeField] private ResourceSummaryDisplay YieldDisplay;

        private IResourceGenerationLogic GenerationLogic;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellSignalLogic signalLogic, IResourceGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IHexCell> tilePossessionCanon
        ){
            signalLogic.BeginHoverSignal.Subscribe(OnBeginHoverFired);
            signalLogic.EndHoverSignal.Subscribe(OnEndHoverFired);

            GenerationLogic = generationLogic;
            CellPossessionCanon = tilePossessionCanon;
        }

        private void OnBeginHoverFired(IHexCell hoveredTile) {
            TerrainTypeField   .text = hoveredTile.Terrain.ToString();
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
