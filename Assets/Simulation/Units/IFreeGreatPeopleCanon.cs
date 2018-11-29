using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public interface IFreeGreatPeopleCanon {

        #region properties

        bool IsActive { get; set; }

        #endregion

        #region methods

        int  GetFreeGreatPeopleForCiv    (ICivilization civ);
        void SetFreeGreatPeopleForCiv    (ICivilization civ, int value);

        void AddFreeGreatPersonToCiv     (ICivilization civ);
        void RemoveFreeGreatPersonFromCiv(ICivilization civ);

        void ClearCiv(ICivilization civ);
        void Clear();

        #endregion

    }

}
