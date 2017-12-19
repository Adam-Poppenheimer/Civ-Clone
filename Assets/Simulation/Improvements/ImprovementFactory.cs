using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementFactory : IImprovementFactory {

        #region instance fields and properties

        private DiContainer Container;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private GameObject ImprovementPrefab;

        #endregion

        #region constructors

        [Inject]
        public ImprovementFactory(DiContainer container,
            IImprovementLocationCanon improvementLocationCanon,
            [Inject(Id = "Improvement Prefab")] GameObject improvementPrefab
        ){
            Container = container;
            ImprovementLocationCanon = improvementLocationCanon;
            ImprovementPrefab = improvementPrefab;
        }

        #endregion

        #region instance methods

        #region from IImprovementFactory

        public IImprovement Create(IImprovementTemplate template, IMapTile location) {
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

            if(!ImprovementLocationCanon.CanChangeOwnerOfPossession(newImprovement, location)) {
                throw new ImprovementCreationException("Cannot assign the new improvement to its intended location");
            }
            ImprovementLocationCanon.ChangeOwnerOfPossession(newImprovement, location);

            return newImprovement;
        }

        #endregion

        #endregion
        
    }

}
