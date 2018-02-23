using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilizationFactory
    /// </summary>
    public class CivilizationFactory : ICivilizationFactory {

        #region instance fields and properties

        #region from ICivilizationFactory

        /// <inheritdoc/>
        public ReadOnlyCollection<ICivilization> AllCivilizations {
            get { return allCivilizations.AsReadOnly(); }
        }
        private List<ICivilization> allCivilizations = new List<ICivilization>();

        #endregion

        private DiContainer Container;

        private CivilizationSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public CivilizationFactory(DiContainer container, CivilizationSignals signals) {
            Container = container;
            Signals   = signals;

            signals.CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IFactory<ICivilization>

        /// <inheritdoc/>
        public ICivilization Create(string name, Color color) {
            if(name == null) {
                throw new ArgumentNullException("name");
            }

            var newCivilization = Container.InstantiateComponentOnNewGameObject<Civilization>();

            newCivilization.Name = name;
            newCivilization.Color = color;

            allCivilizations.Add(newCivilization);

            Signals.NewCivilizationCreatedSignal.OnNext(newCivilization);

            return newCivilization;
        }        

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            allCivilizations.Remove(civ);
        }

        #endregion

    }

}
