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
                    VisibilitySignals.CellVisibilityModeChanged.OnNext(new UniRx.Unit());
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
                    VisibilitySignals.ResourceVisibilityModeChanged.OnNext(new UniRx.Unit());
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
                case CellVisibilityMode.ActiveCiv: return GameCore.ActiveCiv != null && IsCellVisibleToCiv(cell, GameCore.ActiveCiv);
                case CellVisibilityMode.RevealAll: return true;
                case CellVisibilityMode.HideAll:   return false;
                default: throw new NotImplementedException();
            }
        }

        public bool IsCellVisibleToCiv(IHexCell cell, ICivilization civ) {
            return GetVisibilityOfCellToCiv(cell, civ) > 0;
        }

        public int GetVisibilityOfCellToCiv(IHexCell cell, ICivilization civ) {
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

        public void DecreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int oldVisibility = GetVisibilityOfCellToCiv(cell, civ);

            int newVisibility = oldVisibility - 1;

            SetVisibility(cell, civ, newVisibility);

            if(oldVisibility == 1) {
                VisibilitySignals.CellBecameInvisibleToCiv.OnNext(new UniRx.Tuple<IHexCell, ICivilization>(cell, civ));
            }
        }

        public void IncreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int oldVisibility = GetVisibilityOfCellToCiv(cell, civ);

            int newVisibility =  oldVisibility + 1;

            SetVisibility(cell, civ, newVisibility);

            if(oldVisibility <= 0 && newVisibility > 0) {
                VisibilitySignals.CellBecameVisibleToCiv.OnNext(new UniRx.Tuple<IHexCell, ICivilization>(cell, civ));
            }
        }

        public void ClearCellVisibility() {
            VisibilityOfCellToCiv.Clear();
        }

        public bool IsResourceVisible(IResourceDefinition resource) {
            switch(ResourceVisibilityMode) {
                case ResourceVisibilityMode.ActiveCiv: return GameCore.ActiveCiv != null && TechCanon.IsResourceDiscoveredByCiv(resource, GameCore.ActiveCiv);
                case ResourceVisibilityMode.RevealAll: return true;
                case ResourceVisibilityMode.HideAll:   return false;
                default: throw new NotImplementedException("No behavior defined for ResourceVisibilityMode " + ResourceVisibilityMode);
            }
        }

        #endregion

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
