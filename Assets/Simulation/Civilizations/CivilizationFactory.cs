using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Technology;

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

        public ICivilization BarbarianCiv {
            get { return AllCivilizations.FirstOrDefault(civ => civ.Template.IsBarbaric); }
        }

        #endregion

        private DiContainer         Container;
        private CivilizationSignals Signals;
        private ITechCanon          TechCanon;
        private Transform           CivContainer;

        #endregion

        #region constructors

        [Inject]
        public CivilizationFactory(
            DiContainer container, CivilizationSignals signals, ITechCanon techCanon,
            [InjectOptional(Id = "Civ Container")] Transform civContainer
        ) {
            Container    = container;
            Signals      = signals;
            TechCanon    = techCanon;
            CivContainer = civContainer;

            signals.CivBeingDestroyed.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IFactory<ICivilization>

        /// <inheritdoc/>
        public ICivilization Create(ICivilizationTemplate template) {
            return Create(template, new List<ITechDefinition>());
        }

        public ICivilization Create(ICivilizationTemplate template, IEnumerable<ITechDefinition> startingTechs) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }

            var newCivilization = Container.InstantiateComponentOnNewGameObject<Civilization>();

            newCivilization.Template = template;

            if(CivContainer != null) {
                newCivilization.transform.SetParent(CivContainer, false);
            }

            allCivilizations.Add(newCivilization);

            foreach(var tech in startingTechs) {
                TechCanon.SetTechAsDiscoveredForCiv(tech, newCivilization);
            }

            Signals.NewCivilizationCreated.OnNext(newCivilization);

            return newCivilization;
        }

        public void Clear() {
            foreach(var civ in allCivilizations) {
                civ.Destroy();
            }

            allCivilizations.Clear();
        }

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            allCivilizations.Remove(civ);
        }

        #endregion

    }

}
