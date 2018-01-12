using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilizationFactory
    /// </summary>
    public class CivilizationFactory : ICivilizationFactory, IValidatable {

        #region instance fields and properties

        #region from ICivilizationFactory

        /// <inheritdoc/>
        public IEnumerable<ICivilization> AllCivilizations {
            get { return allCivilizations.AsReadOnly(); }
        }
        private List<ICivilization> allCivilizations = new List<ICivilization>();

        #endregion

        private DiContainer Container;        

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        [Inject]
        public CivilizationFactory(DiContainer container) {
            Container = container;
        }

        #endregion

        #region instance methods

        #region from IFactory<ICivilization>

        /// <inheritdoc/>
        public ICivilization Create(string name) {
            if(name == null) {
                throw new ArgumentNullException("name");
            }

            var newCivilization = Container.Instantiate<Civilization>(new List<object>() { name });

            allCivilizations.Add(newCivilization);

            return newCivilization;
        }        

        #endregion

        #region from IValidatable

        /// <inheritdoc/>
        public void Validate() {
            Container.Instantiate<Civilization>();
        }

        #endregion

        #endregion

    }

}
