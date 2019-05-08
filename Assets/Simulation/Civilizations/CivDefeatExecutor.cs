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

        public bool IsActive { get; set; }

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

            CivSignals.CivLostUnit.Subscribe(OnLostUnit);
            CivSignals.CivLostCity.Subscribe(OnLostCity);

            CivSignals.NewCivilizationCreated.Subscribe(OnNewCivCreated);
        }

        #endregion

        #region instance methods

        #region from ICivDefeatExecutor

        public void PerformDefeatOfCiv(ICivilization civ) {
            CivSignals.CivDefeated.OnNext(civ);

            CivsToCheck.Remove(civ);

            civ.Destroy();
        }

        public bool ShouldCivBeDefeated(ICivilization civ) {
            if(!IsActive || !CivsToCheck.Contains(civ)) {
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

        public bool IsCheckingCiv(ICivilization civ) {
            return CivsToCheck.Contains(civ);
        }

        #endregion

        private void OnLostUnit(UniRx.Tuple<ICivilization, IUnit> data) {
            if(ShouldCivBeDefeated(data.Item1)) {
                PerformDefeatOfCiv(data.Item1);
            }
        }

        private void OnLostCity(UniRx.Tuple<ICivilization, ICity> data) {
            if(ShouldCivBeDefeated(data.Item1)) {
                PerformDefeatOfCiv(data.Item1);
            }
        }

        private void OnNewCivCreated(ICivilization civ) {
            if(!civ.Template.IsBarbaric) {
                CivsToCheck.Add(civ);
            }
        }

        #endregion

    }

}
