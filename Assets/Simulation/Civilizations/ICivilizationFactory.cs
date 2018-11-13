using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// Defines a factory capable of creating civilizations of a particular name.
    /// </summary>
    public interface ICivilizationFactory {

        #region properties

        /// <summary>
        /// All civilizations created or recognized by this factory.
        /// </summary>
        ReadOnlyCollection<ICivilization> AllCivilizations { get; }

        #endregion

        #region methods

        ICivilization Create(ICivilizationTemplate template);

        void Clear();

        #endregion

    }

}
