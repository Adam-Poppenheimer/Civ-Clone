using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class CompositeUnitSignals {

        #region instance fields and properties

        public IObservable<IUnit> ActiveCivUnitClickedSignal { get; private set; }

        private IGameCore GameCore;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CompositeUnitSignals(UnitSignals signals, IGameCore gameCore,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ){
            GameCore            = gameCore;
            UnitPossessionCanon = unitPossessionCanon;

            ActiveCivUnitClickedSignal = signals.ClickedSignal.Where(ActiveCivUnitFilter);
        }

        #endregion

        #region instance methods

        private bool ActiveCivUnitFilter(IUnit unit) {
            return UnitPossessionCanon.GetOwnerOfPossession(unit) == GameCore.ActiveCivilization;
        }

        #endregion

    }

}
