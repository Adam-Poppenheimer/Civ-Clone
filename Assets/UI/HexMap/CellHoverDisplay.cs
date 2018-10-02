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
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Visibility;
using Assets.Simulation.Core;

using Assets.UI.SpecialtyResources;

namespace Assets.UI.HexMap {

    public class CellHoverDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text CellDataField;

        [SerializeField] private YieldSummaryDisplay YieldDisplay;

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
         



        private IHexCellSignalLogic                              SignalLogic;
        private IYieldGenerationLogic                            GenerationLogic;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodePositionCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private ICellYieldLogic                                  CellYieldLogic;
        private IGameCore                                        GameCore;
        private IVisibilityCanon                                 VisibilityCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellSignalLogic                              signalLogic,
            IYieldGenerationLogic                            generationLogic,
            IPossessionRelationship<ICity, IHexCell>         cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodePositionCanon,
            IImprovementLocationCanon                        improvementLocationCanon,
            ICellYieldLogic                                  cellResourceLogic,
            IGameCore                                        gameCore,
            IVisibilityCanon                                 visibilityCanon
        ){
            SignalLogic               = signalLogic;
            GenerationLogic           = generationLogic;
            CellPossessionCanon       = cellPossessionCanon;
            ResourceNodePositionCanon = resourceNodePositionCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            CellYieldLogic            = cellResourceLogic;
            GameCore                  = gameCore;
            VisibilityCanon           = visibilityCanon;
        }

        private void OnBeginHoverFired(IHexCell hoveredCell) {
            SetCellDataField      (hoveredCell);
            SetYieldDisplay       (hoveredCell);
            SetResourceNodeDisplay(hoveredCell);
            
            transform.position = Camera.main.WorldToScreenPoint(hoveredCell.Position);

            gameObject.SetActive(true);            
        }

        private void OnEndHoverFired(IHexCell unhoveredTile) {
            gameObject.SetActive(false);
        }

        private void SetCellDataField(IHexCell cell) {
            string cellDataString = cell.Terrain.ToString();

            if(cell.Shape != CellShape.Flatlands) {
                cellDataString += ", " + cell.Shape.ToString();
            }

            if(cell.Vegetation != CellVegetation.None) {
                cellDataString += ", " + cell.Vegetation.ToString();
            }

            if(cell.Feature != CellFeature.None) {
                cellDataString += ", " + cell.Feature.ToString();
            }

            cellDataString += GetImprovementString(cell);

            CellDataField.text = cellDataString;
        }

        private string GetImprovementString(IHexCell cell) {
            string retval = "";

            var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(improvementAtLocation != null) {
                retval += ", " + improvementAtLocation.Template.name;

                if(!improvementAtLocation.IsConstructed) {
                    retval += string.Format(
                        " (unfinished: {0}/{1})",
                        improvementAtLocation.WorkInvested, improvementAtLocation.Template.TurnsToConstruct
                    );
                }else if(improvementAtLocation.IsPillaged) {
                    retval += string.Format(" (pillaged)");
                }
            }

            return retval;
        }

        private void SetYieldDisplay(IHexCell cell) {
            var cellOwner = CellPossessionCanon.GetOwnerOfPossession(cell);

            if(cellOwner != null) {
                YieldDisplay.DisplaySummary(GenerationLogic.GetYieldOfCellForCity(cell, cellOwner));
            }else {
                YieldDisplay.DisplaySummary(CellYieldLogic.GetYieldOfCell(cell, GameCore.ActiveCivilization));
            }
        }

        private void SetResourceNodeDisplay(IHexCell cell) {
            var resourceNodeAt = ResourceNodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(resourceNodeAt != null) {
                if(VisibilityCanon.IsResourceVisible(resourceNodeAt.Resource)) {
                    ResourceNodeDisplay.gameObject.SetActive(true);
                    ResourceNodeDisplay.DisplayNode(resourceNodeAt);
                }else {
                    ResourceNodeDisplay.gameObject.SetActive(false);
                }
            }else {
                ResourceNodeDisplay.gameObject.SetActive(false);
            }
        }

        #endregion

    }

}
