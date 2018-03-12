﻿using System;
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
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;

using Assets.UI.SpecialtyResources;

namespace Assets.UI.HexMap {

    public class CellHoverDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text CellDataField;

        [SerializeField] private ResourceSummaryDisplay YieldDisplay;

        [SerializeField] private ResourceNodeDisplay ResourceNodeDisplay;

        public bool IsPermittedToActivate {
            get { return _isPermittedToActivate; }
            set {
                if(_isPermittedToActivate != value) {
                    _isPermittedToActivate = value;

                    if(_isPermittedToActivate) {
                        BeginHoverSubscription = SignalLogic.BeginHoverSignal.Subscribe(OnBeginHoverFired);
                        EndHoverSubscription   = SignalLogic.EndHoverSignal  .Subscribe(OnEndHoverFired);
                    }else {
                        if(BeginHoverSubscription != null) BeginHoverSubscription.Dispose();
                        if(EndHoverSubscription    != null) EndHoverSubscription .Dispose();
                    }
                }
            }
        }
        private bool _isPermittedToActivate;

        private IDisposable BeginHoverSubscription;
        private IDisposable EndHoverSubscription;
         



        private IHexCellSignalLogic SignalLogic;

        private IResourceGenerationLogic GenerationLogic;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodePositionCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellSignalLogic signalLogic, IResourceGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodePositionCanon,
            IImprovementLocationCanon improvementLocationCanon
        ){
            SignalLogic               = signalLogic;
            GenerationLogic           = generationLogic;
            CellPossessionCanon       = cellPossessionCanon;
            ResourceNodePositionCanon = resourceNodePositionCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
        }

        private void OnBeginHoverFired(IHexCell hoveredCell) {
            SetCellDataField      (hoveredCell);
            SetYieldDisplay       (hoveredCell);
            SetResourceNodeDisplay(hoveredCell);
            
            transform.position = Camera.main.WorldToScreenPoint(hoveredCell.transform.position);

            gameObject.SetActive(true);            
        }

        private void OnEndHoverFired(IHexCell unhoveredTile) {
            gameObject.SetActive(false);
        }

        private void SetCellDataField(IHexCell cell) {
            string cellDataString = GetTerrainName(cell);

            if(cell.Shape != TerrainShape.Flatlands) {
                cellDataString += ", " + cell.Shape.ToString();
            }

            if(cell.Feature != TerrainFeature.None) {
                cellDataString += ", " + cell.Feature.ToString();
            }

            var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(improvementAtLocation != null) {
                cellDataString += ", " + improvementAtLocation.Template.name;
            }

            CellDataField.text = cellDataString;
        }

        private string GetTerrainName(IHexCell cell) {
            if(cell.IsUnderwater) {
                return "Water";
            }else {
                return cell.Terrain.ToString();
            }
        }

        private void SetYieldDisplay(IHexCell cell) {
            var cellOwner = CellPossessionCanon.GetOwnerOfPossession(cell);

            if(cellOwner != null) {
                YieldDisplay.DisplaySummary(GenerationLogic.GetYieldOfSlotForCity(cell.WorkerSlot, cellOwner));
            }else {
                YieldDisplay.DisplaySummary(cell.WorkerSlot.BaseYield);
            }
        }

        private void SetResourceNodeDisplay(IHexCell cell) {
            var resourceNodeAt = ResourceNodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(resourceNodeAt != null) {
                ResourceNodeDisplay.gameObject.SetActive(true);
                ResourceNodeDisplay.DisplayNode(resourceNodeAt);
            }else {
                ResourceNodeDisplay.gameObject.SetActive(false);
            }
        }

        #endregion

    }

}
