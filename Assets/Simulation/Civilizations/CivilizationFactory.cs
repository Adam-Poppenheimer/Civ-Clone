using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Civilizations {

    public class CivilizationFactory : IFactory<string, ICivilization>, IValidatable {

        #region instance fields and properties

        public IEnumerable<ICivilization> AllCivilizations {
            get { return allCivilizations.AsReadOnly(); }
        }
        private List<ICivilization> allCivilizations = new List<ICivilization>();

        private DiContainer Container;        

        #endregion

        #region constructors

        [Inject]
        public CivilizationFactory(DiContainer container) {
            Container = container;
        }

        #endregion

        #region instance methods

        #region from IFactory<ICivilization>

        public ICivilization Create(string name) {
            var newCivilization = Container.Instantiate<Civilization>(new List<object>() { name });

            allCivilizations.Add(newCivilization);

            return newCivilization;
        }        

        #endregion

        #region from IValidatable

        public void Validate() {
            Container.Instantiate<Civilization>();
        }

        #endregion

        #endregion

    }

}
