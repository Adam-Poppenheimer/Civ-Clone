using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Core;

namespace Assets.Simulation.Civilizations {

    public class GreatMilitaryPointGainLogic : IPlayModeSensitiveElement {

        #region instance fields and properties

        #region from IGreatMilitaryPointGainLogic

        public bool IsActive {
            get { return _isActive; }
            set {
                if(_isActive != value) {
                    _isActive = value;

                    if(_isActive) {
                        UnitGainedExperienceSubscription = UnitSignals.GainedExperience.Subscribe(OnUnitGainedExperience);
                    }else {
                        UnitGainedExperienceSubscription.Dispose();
                        UnitGainedExperienceSubscription = null;
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private IDisposable UnitGainedExperienceSubscription;



        private IGreatPersonCanon                             GreatPersonCanon;
        private ICivilizationConfig                           CivConfig;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private UnitSignals                                   UnitSignals;
        private ICivModifiers                                 CivModifiers;

        #endregion

        #region constructors

        [Inject]
        public GreatMilitaryPointGainLogic(
            IGreatPersonCanon greatPersonCanon, ICivilizationConfig civConfig,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            UnitSignals unitSignals, ICivModifiers civModifiers
        ) {
            GreatPersonCanon    = greatPersonCanon;
            CivConfig           = civConfig;
            UnitPossessionCanon = unitPossessionCanon;
            UnitSignals         = unitSignals;
            CivModifiers        = civModifiers;
        }

        #endregion

        #region instance methods

        private void OnUnitGainedExperience(Tuple<IUnit, int> data) {
            var unit = data.Item1;
            int experienceGained = data.Item2;

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            float modifier = CivModifiers.GreatMilitaryGainSpeed.GetValueForCiv(unitOwner);

            if(unit.Type == UnitType.NavalMelee || unit.Type == UnitType.NavalRanged) {
                GreatPersonCanon.AddPointsTowardsTypeForCiv(
                    GreatPersonType.GreatAdmiral, unitOwner,
                    experienceGained * CivConfig.ExperienceToGreatPersonPointRatio * modifier
                );
            }else if(unit.Type != UnitType.Civilian && unit.Type != UnitType.City) {
                GreatPersonCanon.AddPointsTowardsTypeForCiv(
                    GreatPersonType.GreatGeneral, unitOwner,
                    experienceGained * CivConfig.ExperienceToGreatPersonPointRatio * modifier
                );
            }
        }

        #endregion

    }

}
