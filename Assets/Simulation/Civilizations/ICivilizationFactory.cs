using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationFactory : IFactory<string, ICivilization> {

        #region properties

        IEnumerable<ICivilization> AllCivilizations { get; }

        #endregion

    }

}
