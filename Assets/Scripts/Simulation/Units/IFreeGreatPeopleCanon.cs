using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.Simulation.Units {

    public interface IFreeGreatPeopleCanon : IPlayModeSensitiveElement {

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
