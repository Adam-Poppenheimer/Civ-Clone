using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Visibility {

    public class VisibilityCanon : IVisibilityCanon {

        #region instance fields and properties

        #region from IVisibilityCanon

        public CellVisibilityMode CellVisibilityMode {
            get { return _cellVisibilityMode; }
            set {
                if(_cellVisibilityMode != value) {
                    _cellVisibilityMode = value;
                    VisibilitySignals.CellVisibilityModeChangedSignal.OnNext(new UniRx.Unit());
                }
            }
        }
        private CellVisibilityMode _cellVisibilityMode;

        public RevealMode RevealMode { get; set; }

        public ResourceVisibilityMode ResourceVisibilityMode {
            get { return _resourceVisibilityMode; }
            set {
                if(_resourceVisibilityMode != value) {
                    _resourceVisibilityMode = value;
                    VisibilitySignals.ResourceVisibilityModeChangedSignal.OnNext(new UniRx.Unit());
                }
            }
        }
        private ResourceVisibilityMode _resourceVisibilityMode;

        #endregion

        private Dictionary<IHexCell, Dictionary<ICivilization, int>> VisibilityOfCellToCiv =
            new Dictionary<IHexCell, Dictionary<ICivilization, int>>();



        private IGameCore         GameCore;
        private ITechCanon        TechCanon;
        private VisibilitySignals VisibilitySignals;

        #endregion

        #region constructors

        [Inject]
        public VisibilityCanon(
            IGameCore gameCore, ITechCanon techCanon, VisibilitySignals visibilitySignals,
            HexCellSignals cellSignals
        ) {
            GameCore          = gameCore;
            TechCanon         = techCanon;
            VisibilitySignals = visibilitySignals;

            cellSignals.MapBeingClearedSignal.Subscribe(unit => ClearCellVisibility());
        }

        #endregion

        #region instance methods

        #region from ICellVisibilityCanon

        public bool IsCellVisible(IHexCell cell) {
            switch(CellVisibilityMode) {
                case CellVisibilityMode.ActiveCiv: return IsCellVisibleToCiv(cell, GameCore.ActiveCivilization);
                case CellVisibilityMode.RevealAll: return true;
                case CellVisibilityMode.HideAll:   return false;
                default: throw new NotImplementedException();
            }
        }

        public bool IsCellVisibleToCiv(IHexCell cell, ICivilization civ) {
            return GetVisibility(cell, civ) > 0;
        }

        public IEnumerable<IHexCell> GetCellsVisibleToCiv(ICivilization civ) {
            throw new NotImplementedException();
        }

        public void DecreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int visibility = GetVisibility(cell, civ) - 1;
            SetVisibility(cell, civ, visibility);
            if(visibility <= 0) {
                cell.RefreshVisibility();
            }
        }

        public void IncreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int visibility =  GetVisibility(cell, civ) + 1;
            SetVisibility(cell, civ, visibility);
            if(visibility >= 1) {
                cell.RefreshVisibility();
            }
        }

        public void ClearCellVisibility() {
            VisibilityOfCellToCiv.Clear();
        }

        public bool IsResourceVisible(IResourceDefinition resource) {
            switch(ResourceVisibilityMode) {
                case ResourceVisibilityMode.ActiveCiv: return TechCanon.IsResourceDiscoveredByCiv(resource, GameCore.ActiveCivilization);
                case ResourceVisibilityMode.RevealAll: return true;
                case ResourceVisibilityMode.HideAll:   return false;
                default: throw new NotImplementedException("No behavior defined for ResourceVisibilityMode " + ResourceVisibilityMode);
            }
        }

        #endregion

        private int GetVisibility(IHexCell cell, ICivilization civ) {
            Dictionary<ICivilization, int> visibilityDictForCell;

            if(!VisibilityOfCellToCiv.ContainsKey(cell)) {
                visibilityDictForCell = new Dictionary<ICivilization, int>();
                VisibilityOfCellToCiv[cell] = visibilityDictForCell;
            }else {
                visibilityDictForCell = VisibilityOfCellToCiv[cell];
            }

            int retval;
            visibilityDictForCell.TryGetValue(civ, out retval);
            return retval;
        }

        private void SetVisibility(IHexCell cell, ICivilization civ, int value) {
            Dictionary<ICivilization, int> visibilityDictForCell;

            if(!VisibilityOfCellToCiv.ContainsKey(cell)) {
                visibilityDictForCell = new Dictionary<ICivilization, int>();
                VisibilityOfCellToCiv[cell] = visibilityDictForCell;
            }else {
                visibilityDictForCell = VisibilityOfCellToCiv[cell];
            }

            visibilityDictForCell[civ] = value;
        }

        #endregion

    }

}
