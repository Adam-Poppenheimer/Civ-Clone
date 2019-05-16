using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class GreatPersonCanon : IGreatPersonCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, float[]> PointsOfCiv = 
            new Dictionary<ICivilization, float[]>();

        private Dictionary<ICivilization, int[]> PredecessorsOfCiv =
            new Dictionary<ICivilization, int[]>();




        private ICivilizationConfig CivConfig;
        private IUnitConfig         UnitConfig;

        #endregion

        #region constructors

        [Inject]
        public GreatPersonCanon(
            ICivilizationConfig civConfig, IUnitConfig unitConfig,
            CivilizationSignals civSignals
        ) {
            CivConfig  = civConfig;
            UnitConfig = unitConfig;

            civSignals.GreatPersonBorn.Subscribe(OnGreatPersonBorn);
        }

        #endregion

        #region instance methods

        #region from IGreatPeopleCanon

        public float GetPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ) {
            var pointList = GetListForCiv(civ, PointsOfCiv);

            return pointList[(int)type];
        }

        public void AddPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ, float pointsAdded) {
            var pointList = GetListForCiv(civ, PointsOfCiv);

            pointList[(int)type] += pointsAdded;
        }

        public void SetPointsTowardsTypeForCiv(GreatPersonType type, ICivilization civ, float newPoints) {
            var pointList = GetListForCiv(civ, PointsOfCiv);

            pointList[(int)type] = newPoints;
        }

        public float GetPointsNeededForTypeForCiv(GreatPersonType type, ICivilization civ) {
            int predecessorsOfGroup = 0;
            float startingCost = 0f;

            if(UnitConfig.GreatPeopleCivilianTypes.Contains(type)) {
                predecessorsOfGroup = UnitConfig.GreatPeopleCivilianTypes.Sum(
                    relatedType => GetPredecessorsOfTypeForCiv(relatedType, civ)
                );

                startingCost = CivConfig.CivilianGreatPersonStartingCost;

            }else if(UnitConfig.GreatPeopleMilitaryTypes.Contains(type)) {
                predecessorsOfGroup = UnitConfig.GreatPeopleMilitaryTypes.Sum(
                    relatedType => GetPredecessorsOfTypeForCiv(relatedType, civ)
                );

                startingCost = CivConfig.MilitaryGreatPersonStartingCost;

            }else {
                predecessorsOfGroup = GetPredecessorsOfTypeForCiv(type, civ);

                startingCost = CivConfig.CivilianGreatPersonStartingCost;
            }

            return startingCost *  Mathf.Pow(CivConfig.GreatPersonPredecessorCostMultiplier, predecessorsOfGroup);
        }

        public int GetPredecessorsOfTypeForCiv(GreatPersonType type, ICivilization civ) {
            var predecessorList = GetListForCiv(civ, PredecessorsOfCiv);

            return predecessorList[(int)type];
        }

        public void Clear() {
            PointsOfCiv      .Clear();
            PredecessorsOfCiv.Clear();
        }

        public void ClearCiv(ICivilization civ) {
            PointsOfCiv      .Remove(civ);
            PredecessorsOfCiv.Remove(civ);
        }

        #endregion

        private T[] GetListForCiv<T>(ICivilization civ, Dictionary<ICivilization, T[]> dict) {
            T[] retval;

            if(!dict.TryGetValue(civ, out retval)) {
                retval = new T[EnumUtil.GetValues<YieldType>().Count()];

                dict[civ] = retval;
            }

            return retval;
        }

        private void OnGreatPersonBorn(GreatPersonBirthData data) {
            var predecessorList = GetListForCiv(data.Owner, PredecessorsOfCiv);

            predecessorList[(int)data.Type]++;
        }

        #endregion

    }

}
