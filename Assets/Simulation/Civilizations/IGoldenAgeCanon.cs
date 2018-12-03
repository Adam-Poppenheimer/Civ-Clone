using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface IGoldenAgeCanon {

        #region methods

        float GetGoldenAgeProgressForCiv(ICivilization civ);
        float GetNextGoldenAgeCostForCiv(ICivilization civ);
        
        void ChangeGoldenAgeProgressForCiv(ICivilization civ, float valueAdded);
        void SetGoldenAgeProgressForCiv   (ICivilization civ, float newValue);

        bool IsCivInGoldenAge             (ICivilization civ);
        int  GetTurnsLeftOnGoldenAgeForCiv(ICivilization civ);

        void StartGoldenAgeForCiv(ICivilization civ, int turns);
        void StopGoldenAgeForCiv (ICivilization civ);

        void ChangeTurnsOfGoldenAgeForCiv(ICivilization civ, int addedTurns);

        int  GetPreviousGoldenAgesForCiv(ICivilization civ);
        void SetPreviousGoldenAgesForCiv(ICivilization civ, int previousAges);

        void ClearCiv(ICivilization civ);
        void Clear();

        #endregion

    }

}
