using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CivDefeatExecutor : ICivDefeatExecutor {

        #region instance fields and properties

        #region from ICivDefeatExecutor

        public bool CheckForDefeat { get; set; }

        #endregion

        private HashSet<ICivilization> CivsToCheck = new HashSet<ICivilization>();



        private ICivilizationConfig                           CivConfig;
        private ICanBuildCityLogic                            CanBuildCityLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private CivilizationSignals                           CivSignals;        

        #endregion

        #region constructors

        [Inject]
        public CivDefeatExecutor(
            ICivilizationConfig civConfig, ICanBuildCityLogic canBuildCityLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            CivilizationSignals civSignals
        ) {
            CivConfig           = civConfig;
            CanBuildCityLogic   = canBuildCityLogic;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            CivSignals          = civSignals;

            CivSignals.CivLostUnitSignal.Subscribe(OnLostUnit);
            CivSignals.CivLostCitySignal.Subscribe(OnLostCity);

            CivSignals.NewCivilizationCreatedSignal.Subscribe(civ => CivsToCheck.Add(civ));
        }

        #endregion

        #region instance methods

        #region from ICivDefeatExecutor

        public void PerformDefeatOfCiv(ICivilization civ) {
            CivSignals.CivDefeatedSignal.OnNext(civ);

            CivsToCheck.Remove(civ);

            civ.Destroy();
        }

        public bool ShouldCivBeDefeated(ICivilization civ) {
            if(!CheckForDefeat || !CivsToCheck.Contains(civ)) {
                return false;
            }

            var citiesOfCiv = CityPossessionCanon.GetPossessionsOfOwner(civ);
            var unitsOfCiv  = UnitPossessionCanon.GetPossessionsOfOwner(civ);

            if(CivConfig.DefeatMode == CivilizationDefeatMode.NoMoreCities) {
                var settlers = unitsOfCiv.Where(unit => CanBuildCityLogic.CanUnitBuildCity(unit));

                return !citiesOfCiv.Any() && !settlers.Any();

            }else if(CivConfig.DefeatMode == CivilizationDefeatMode.NoMoreCitiesOrUnits) {
                return !citiesOfCiv.Any() && !unitsOfCiv.Any();

            }else {
                throw new NotImplementedException();
            }
        }

        #endregion

        private void OnLostUnit(Tuple<ICivilization, IUnit> data) {
            if(ShouldCivBeDefeated(data.Item1)) {
                PerformDefeatOfCiv(data.Item1);
            }
        }

        private void OnLostCity(Tuple<ICivilization, ICity> data) {
            if(ShouldCivBeDefeated(data.Item1)) {
                PerformDefeatOfCiv(data.Item1);
            }
        }

        #endregion

    }

}
