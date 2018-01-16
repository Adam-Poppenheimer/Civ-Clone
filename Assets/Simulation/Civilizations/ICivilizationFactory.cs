using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// Defines a factory capable of creating civilizations of a particular name.
    /// </summary>
    public interface ICivilizationFactory : IFactory<string, ICivilization> {

        #region properties

        /// <summary>
        /// All civilizations created or recognized by this factory.
        /// </summary>
        ReadOnlyCollection<ICivilization> AllCivilizations { get; }

        #endregion

    }

}
