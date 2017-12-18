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

        private IPossessionRelationship<IMapTile, IImprovement> ImprovementLocationCanon;

        private GameObject ImprovementPrefab;

        #endregion

        #region constructors

        [Inject]
        public ImprovementFactory(DiContainer container,
            IPossessionRelationship<IMapTile, IImprovement> improvementLocationCanon,
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

            Container.InjectGameObject(newGameObject);

            var newImprovement = newGameObject.GetComponent<Improvement>();
            newImprovement.Template = template;

            ImprovementLocationCanon.ChangeOwnerOfPossession(newImprovement, location);

            return newImprovement;
        }

        #endregion

        #endregion
        
    }

}
