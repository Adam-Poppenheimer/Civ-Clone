using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources{

    public class ResourceNodeFactory : IResourceNodeFactory {

        #region instance fields and properties

        #region from IResourceNodeFactory

        public IEnumerable<IResourceNode> AllNodes {
            get { return allNodes; }
        }
        private List<IResourceNode> allNodes = new List<IResourceNode>();

        #endregion

        private DiContainer Container;

		private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;

        

        #endregion

        #region constructors

        [Inject]
		public ResourceNodeFactory(DiContainer container,
			IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
             SpecialtyResourceSignals signals
		){
			Container                 = container;
			ResourceNodeLocationCanon = resourceNodeLocationCanon;

            signals.ResourceNodeBeingDestroyedSignal.Subscribe(OnNodeBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IResourceNodeFactory

		public bool CanBuildNode(IHexCell location, ISpecialtyResourceDefinition definition) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(definition == null) {
                throw new ArgumentNullException("definition");
            }
            return ResourceNodeLocationCanon.GetPossessionsOfOwner(location).Count() == 0;
        }

		public IResourceNode BuildNode(IHexCell location, ISpecialtyResourceDefinition definition, int copies) {
			if(!CanBuildNode(location, definition)) {
				throw new InvalidOperationException("CanBuildNode must return true on the argued location and definition");
            }

			var newNode = Container.InstantiateComponentOnNewGameObject<ResourceNode>();

			newNode.transform.SetParent(location.transform, false);

			newNode.Resource = definition;
			newNode.Copies   = copies;

			ResourceNodeLocationCanon.ChangeOwnerOfPossession(newNode, location);

            allNodes.Add(newNode);

			return newNode;
        }

        #endregion

        private void OnNodeBeingDestroyed(IResourceNode node) {
            allNodes.Remove(node);
        }

        #endregion
        
    }

}
