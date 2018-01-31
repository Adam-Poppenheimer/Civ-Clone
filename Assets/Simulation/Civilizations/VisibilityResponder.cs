using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Civilizations {

    public class VisibilityResponder {

        #region instance fields and properties

        private IHexGrid Grid;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private ICellVisibilityCanon VisibilityCanon;

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public VisibilityResponder(
            UnitSignals unitSignals, CitySignals citySignals, IHexGrid grid,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICellVisibilityCanon visibilityCanon, IUnitPositionCanon unitPositionCanon
        ){
            unitSignals.LeftLocationSignal      .Subscribe(OnUnitLeftLocation);
            unitSignals.EnteredLocationSignal   .Subscribe(OnUnitEnteredLocation);

            citySignals.LostCellFromBoundariesSignal.Subscribe(OnCityLostCell);
            citySignals.GainedCellToBoundariesSignal.Subscribe(OnCityGainedCell);

            Grid                = grid;
            UnitPossessionCanon = unitPossessionCanon;
            CityPossessionCanon = cityPossessionCanon;
            VisibilityCanon     = visibilityCanon;
            UnitPositionCanon   = unitPositionCanon;
        }

        #endregion

        #region instance methods

        private void OnUnitLeftLocation(Tuple<IUnit, IHexCell> args) {
            var unit = args.Item1;
            var oldLocation = args.Item2;

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(unitOwner != null && oldLocation != null) {
                foreach(var cell in Grid.GetCellsInRadius(oldLocation, unit.VisionRange)) {
                    VisibilityCanon.DecreaseVisibilityToCiv(cell, unitOwner);
                }
            }
        }

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> args) {
            var unit = args.Item1;
            var newLocation = args.Item2;

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(unitOwner != null && newLocation != null) {
                foreach(var cell in Grid.GetCellsInRadius(newLocation, unit.VisionRange)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(cell, unitOwner);
                }
            }
        }

        private void OnCityLostCell(Tuple<ICity, IHexCell> args) {
            var city = args.Item1;
            var oldCell = args.Item2;

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            if(cityOwner != null && oldCell != null) {
                foreach(var cell in Grid.GetCellsInRadius(oldCell, 1)) {
                    VisibilityCanon.DecreaseVisibilityToCiv(cell, cityOwner);
                }
            }
        }

        private void OnCityGainedCell(Tuple<ICity, IHexCell> args) {
            var city = args.Item1;
            var newCell = args.Item2;

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            if(cityOwner != null && newCell != null) {
                foreach(var cell in Grid.GetCellsInRadius(newCell, 1)) {
                    VisibilityCanon.IncreaseVisibilityToCiv(cell, cityOwner);
                }
            }
        }

        #endregion

    }

}
