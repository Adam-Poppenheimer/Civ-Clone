using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Visibility;
using Assets.Simulation.Units;
using Assets.Simulation.Core;

namespace Assets.Simulation.Civilizations {

    public class CivDiscoveryResponder {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private ICivDiscoveryCanon                            CivDiscoveryCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CivDiscoveryResponder(
            IUnitPositionCanon unitPositionCanon, ICivDiscoveryCanon civDiscoveryCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            VisibilitySignals visibilitySignals
        ) {
            UnitPositionCanon   = unitPositionCanon;
            CivDiscoveryCanon   = civDiscoveryCanon;
            UnitPossessionCanon = unitPossessionCanon;

            visibilitySignals.CellBecameExploredByCiv.Subscribe(OnCellBecameExploredByCiv);
        }

        #endregion

        #region instance methods

        private void OnCellBecameExploredByCiv(Tuple<IHexCell, ICivilization> data) {
            var cell = data.Item1;
            var civ  = data.Item2;

            foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cell)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                if(CivDiscoveryCanon.CanEstablishDiscoveryBetweenCivs(civ, unitOwner)) {
                    CivDiscoveryCanon.EstablishDiscoveryBetweenCivs(civ, unitOwner);
                }
            }
        }

        #endregion

    }

}
