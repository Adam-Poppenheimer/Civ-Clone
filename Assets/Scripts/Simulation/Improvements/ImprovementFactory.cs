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

        private DiContainer               Container;
        private IImprovementLocationCanon ImprovementLocationCanon;
        private ICellModificationLogic    CellModificationLogic;
        private GameObject                ImprovementPrefab;

        #endregion

        #region constructors

        [Inject]
        public ImprovementFactory(DiContainer container,
            IImprovementLocationCanon improvementLocationCanon,
            ICellModificationLogic cellModificationLogic,
            [Inject(Id = "Improvement Prefab")] GameObject improvementPrefab,
            ImprovementSignals signals
        ){
            Container                = container;
            ImprovementLocationCanon = improvementLocationCanon;
            CellModificationLogic    = cellModificationLogic;
            ImprovementPrefab        = improvementPrefab;

            signals.BeingDestroyed.Subscribe(OnImprovementBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IImprovementFactory

        public IImprovement BuildImprovement(IImprovementTemplate template, IHexCell location) {
            return BuildImprovement(template, location, 0, false, false);
        }

        public IImprovement BuildImprovement(
            IImprovementTemplate template, IHexCell location, float workInvested,
            bool isConstructed, bool isPillaged
        ){
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(location == null) {
                throw new ArgumentNullException("location");
            }

            var newGameObject = GameObject.Instantiate(ImprovementPrefab);

            Container.InjectGameObject(newGameObject);

            var newImprovement = newGameObject.GetComponent<Improvement>();
            newImprovement.Template = template;

            if(template.ClearsVegetationWhenBuilt && location.Vegetation != CellVegetation.None) {
                CellModificationLogic.ChangeVegetationOfCell(location, CellVegetation.None);
            }

            if(!ImprovementLocationCanon.CanChangeOwnerOfPossession(newImprovement, location)) {
                throw new ImprovementCreationException("Cannot assign the new improvement to its intended location");
            }
            ImprovementLocationCanon.ChangeOwnerOfPossession(newImprovement, location);

            if(isConstructed) {
                newImprovement.Construct();
            }else if(isPillaged) {
                newImprovement.Pillage();
            }

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
