using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class GoldenAgeCanon : IGoldenAgeCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, float> GoldenAgeProgressForCiv =
            new Dictionary<ICivilization, float>();

        private Dictionary<ICivilization, int> PreviousGoldenAgesForCiv =
            new Dictionary<ICivilization, int>();

        private Dictionary<ICivilization, int> GoldenAgeTurnsLeftForCiv =
            new Dictionary<ICivilization, int>();




        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICivilizationConfig                           CivConfig;        
        private CivilizationSignals                           CivSignals;
        private ICivModifiers                                 CivModifiers;

        #endregion

        #region constructors

        [Inject]
        public GoldenAgeCanon(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICivilizationConfig civConfig, CivilizationSignals civSignals,
            ICivModifiers civModifiers
        ) {
            CityPossessionCanon = cityPossessionCanon;
            CivConfig           = civConfig;
            CivSignals          = civSignals;
            CivModifiers        = civModifiers;
        }

        #endregion

        #region instance methods

        #region from IGoldenAgeCanon

        public float GetGoldenAgeProgressForCiv(ICivilization civ) {
            float retval;

            GoldenAgeProgressForCiv.TryGetValue(civ, out retval);

            return retval;
        }

        public float GetNextGoldenAgeCostForCiv(ICivilization civ) {
            int cityCount = CityPossessionCanon.GetPossessionsOfOwner(civ).Count();

            float modifierFromCities = 1f + cityCount * CivConfig.GoldenAgePerCityMultiplier;

            float costFromPreviousAges = CivConfig.GoldenAgeCostPerPreviousAge * GetPreviousGoldenAgesForCiv(civ);

            return (CivConfig.GoldenAgeBaseCost + costFromPreviousAges) * modifierFromCities;
        }

        public void ChangeGoldenAgeProgressForCiv(ICivilization civ, float valueAdded) {
            float oldValue;

            GoldenAgeProgressForCiv.TryGetValue(civ, out oldValue);

            GoldenAgeProgressForCiv[civ] = oldValue + valueAdded;
        }

        public void SetGoldenAgeProgressForCiv(ICivilization civ, float newValue) {
            GoldenAgeProgressForCiv[civ] = newValue;
        }

        public bool IsCivInGoldenAge(ICivilization civ) {
            return GoldenAgeTurnsLeftForCiv.ContainsKey(civ);
        }

        public int GetTurnsLeftOnGoldenAgeForCiv(ICivilization civ) {
            int retval;

            GoldenAgeTurnsLeftForCiv.TryGetValue(civ, out retval);

            return retval;
        }

        public void StartGoldenAgeForCiv(ICivilization civ, int turns) {
            if(IsCivInGoldenAge(civ)) {
                throw new InvalidOperationException("Civ already in golden age");
            }

            GoldenAgeTurnsLeftForCiv[civ] = turns;

            int previousGoldenAges;

            PreviousGoldenAgesForCiv.TryGetValue(civ, out previousGoldenAges);

            previousGoldenAges++;

            PreviousGoldenAgesForCiv[civ] = previousGoldenAges;

            CivSignals.CivEnteredGoldenAge.OnNext(civ);
        }

        public void StopGoldenAgeForCiv(ICivilization civ) {
            if(!IsCivInGoldenAge(civ)) {
                throw new InvalidOperationException("Civ not currently in a golden age");
            }

            GoldenAgeTurnsLeftForCiv.Remove(civ);

            CivSignals.CivLeftGoldenAge.OnNext(civ);
        }

        public void ChangeTurnsOfGoldenAgeForCiv(ICivilization civ, int addedTurns) {
            if(!IsCivInGoldenAge(civ)) {
                throw new InvalidOperationException("Civ not currently in a golden age");
            }

            GoldenAgeTurnsLeftForCiv[civ] += addedTurns;
        }

        public int GetPreviousGoldenAgesForCiv(ICivilization civ) {
            int retval;

            PreviousGoldenAgesForCiv.TryGetValue(civ, out retval);

            return retval;
        }

        public void SetPreviousGoldenAgesForCiv(ICivilization civ, int previousAges) {
            PreviousGoldenAgesForCiv[civ] = previousAges;
        }

        public int GetGoldenAgeLengthForCiv(ICivilization civ) {
            return Mathf.RoundToInt(
                CivConfig.GoldenAgeBaseLength * CivModifiers.GoldenAgeLength.GetValueForCiv(civ)
            );
        }

        public void ClearCiv(ICivilization civ) {
            GoldenAgeTurnsLeftForCiv.Remove(civ);
            PreviousGoldenAgesForCiv.Remove(civ);
            GoldenAgeTurnsLeftForCiv.Remove(civ);
        }

        public void Clear() {
            GoldenAgeTurnsLeftForCiv.Clear();
            PreviousGoldenAgesForCiv.Clear();
            GoldenAgeTurnsLeftForCiv.Clear();
        }

        #endregion

        #endregion

    }

}
