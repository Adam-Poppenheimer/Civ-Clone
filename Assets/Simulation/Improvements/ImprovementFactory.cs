using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementFactory : IImprovementFactory {

        #region instance fields and properties

        #region from IImprovementFactory

        public IEnumerable<IImprovement> AllImprovements {
            get { return allImprovements; }
        }
        private List<IImprovement> allImprovements = new List<IImprovement>();

        #endregion

        private DiContainer Container;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private GameObject ImprovementPrefab;

        #endregion

        #region constructors

        [Inject]
        public ImprovementFactory(DiContainer container,
            IImprovementLocationCanon improvementLocationCanon,
            [Inject(Id = "Improvement Prefab")] GameObject improvementPrefab,
            ImprovementSignals signals
        ){
            Container = container;
            ImprovementLocationCanon = improvementLocationCanon;
            ImprovementPrefab = improvementPrefab;

            signals.ImprovementBeingDestroyedSignal.Subscribe(OnImprovementBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IImprovementFactory

        public IImprovement Create(IImprovementTemplate template, IHexCell location) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(location == null) {
                throw new ArgumentNullException("location");
            }

            var newGameObject = GameObject.Instantiate(ImprovementPrefab);
            newGameObject.transform.SetParent(location.transform, false);

            Container.InjectGameObject(newGameObject);

            var newImprovement = newGameObject.GetComponent<Improvement>();
            newImprovement.Template = template;

            if(template.ClearsForestsWhenBuilt && location.Feature == TerrainFeature.Forest) {
                location.Feature = TerrainFeature.None;
            }

            if(!ImprovementLocationCanon.CanChangeOwnerOfPossession(newImprovement, location)) {
                throw new ImprovementCreationException("Cannot assign the new improvement to its intended location");
            }
            ImprovementLocationCanon.ChangeOwnerOfPossession(newImprovement, location);

            allImprovements.Add(newImprovement);

            return newImprovement;
        }

        #endregion

        private void OnImprovementBeingDestroyed(IImprovement improvement) {
            allImprovements.Remove(improvement);
        }

        #endregion
        
    }

}
