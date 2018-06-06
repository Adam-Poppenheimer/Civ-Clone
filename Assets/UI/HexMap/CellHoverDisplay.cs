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
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.Core;

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
         



        private IHexCellSignalLogic                              SignalLogic;
        private IResourceGenerationLogic                         GenerationLogic;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodePositionCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private ICellResourceLogic                               CellResourceLogic;
        private ITechCanon                                       TechCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private IGameCore                                        GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexCellSignalLogic signalLogic, IResourceGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodePositionCanon,
            IImprovementLocationCanon improvementLocationCanon,
            ICellResourceLogic cellResourceLogic, ITechCanon techCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IGameCore gameCore
        ){
            SignalLogic               = signalLogic;
            GenerationLogic           = generationLogic;
            CellPossessionCanon       = cellPossessionCanon;
            ResourceNodePositionCanon = resourceNodePositionCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            CellResourceLogic         = cellResourceLogic;
            TechCanon                 = techCanon;
            CityPossessionCanon       = cityPossessionCanon;
            GameCore                  = gameCore;
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

            if(cell.Shape != TerrainShape.Flatlands) {
                cellDataString += ", " + cell.Shape.ToString();
            }

            if(cell.Feature != TerrainFeature.None) {
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
                YieldDisplay.DisplaySummary(CellResourceLogic.GetYieldOfCell(cell));
            }
        }

        private void SetResourceNodeDisplay(IHexCell cell) {
            var resourceNodeAt = ResourceNodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(resourceNodeAt != null) {
                ICity cityOwningCell = null;
                ICivilization civOwningCell = null;

                cityOwningCell = CellPossessionCanon.GetOwnerOfPossession(cell);

                if(cityOwningCell != null) {
                    civOwningCell = CityPossessionCanon.GetOwnerOfPossession(cityOwningCell);
                }

                if(civOwningCell != null && TechCanon.IsResourceVisibleToCiv(resourceNodeAt.Resource, civOwningCell)) {
                    ResourceNodeDisplay.gameObject.SetActive(true);
                    ResourceNodeDisplay.DisplayNode(resourceNodeAt);

                }else if(
                    GameCore.ActiveCivilization != null &&
                    TechCanon.IsResourceVisibleToCiv(resourceNodeAt.Resource, GameCore.ActiveCivilization)
                ){
                    ResourceNodeDisplay.gameObject.SetActive(true);
                    ResourceNodeDisplay.DisplayNode(resourceNodeAt);
                }
            }else {
                ResourceNodeDisplay.gameObject.SetActive(false);
            }
        }

        #endregion

    }

}
