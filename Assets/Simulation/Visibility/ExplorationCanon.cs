using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Core;

namespace Assets.Simulation.Visibility {

    public class ExplorationCanon : IExplorationCanon {

        #region instance fields and properties

        #region from IExplorationCanon

        public CellExplorationMode ExplorationMode {
            get { return _explorationMode; }
            set {
                if(_explorationMode != value) {
                    _explorationMode = value;
                    VisibilitySignals.CellExplorationModeChangedSignal.OnNext(new UniRx.Unit());
                }
            }
        }
        private CellExplorationMode _explorationMode;

        #endregion

        private Dictionary<IHexCell, HashSet<ICivilization>> CivsHavingExploredCell = 
            new Dictionary<IHexCell, HashSet<ICivilization>>();



        private IGameCore         GameCore;
        private VisibilitySignals VisibilitySignals;
        private HexCellSignals    CellSignals;

        #endregion

        #region constructors

        [Inject]
        public ExplorationCanon(
            IGameCore gameCore, VisibilitySignals visibilitySignals, HexCellSignals cellSignals
        ) {
            GameCore          = gameCore;
            VisibilitySignals = visibilitySignals;
            CellSignals       = cellSignals;

            CellSignals.MapBeingClearedSignal.Subscribe(unit => Clear());
        }

        #endregion

        #region instance methods

        #region from IExplorationCanon

        public bool IsCellExplored(IHexCell cell) {
            switch(ExplorationMode) {
                case CellExplorationMode.ActiveCiv:        return GameCore.ActiveCivilization != null && IsCellExploredByCiv(cell, GameCore.ActiveCivilization);
                case CellExplorationMode.AllCellsExplored: return true;
                default: throw new NotImplementedException();
            }
        }

        public bool IsCellExploredByCiv(IHexCell cell, ICivilization civ) {
            HashSet<ICivilization> explorationData;

            return CivsHavingExploredCell.TryGetValue(cell, out explorationData) && explorationData.Contains(civ);
        }

        public void SetCellAsExploredByCiv(IHexCell cell, ICivilization civ) {
            HashSet<ICivilization> explorationData;

            if(!CivsHavingExploredCell.TryGetValue(cell, out explorationData)) {
                explorationData = new HashSet<ICivilization>();
                CivsHavingExploredCell[cell] = explorationData;
            }

            explorationData.Add(civ);
        }

        public void SetCellAsUnexploredByCiv(IHexCell cell, ICivilization civ) {
            HashSet<ICivilization> explorationData;

            if(CivsHavingExploredCell.TryGetValue(cell, out explorationData)) {
                explorationData.Remove(civ);
            }
        }

        public void Clear() {
            CivsHavingExploredCell.Clear();
        }

        #endregion

        #endregion
    }

}
