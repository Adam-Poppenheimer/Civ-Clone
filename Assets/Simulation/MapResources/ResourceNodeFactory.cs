using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources{

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
        private IResourceRestrictionCanon                        RestrictionCanon;        

        #endregion

        #region constructors

        [Inject]
		public ResourceNodeFactory(
            DiContainer container, IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            IResourceRestrictionCanon restrictionCanon, ResourceSignals signals
		){
			Container                 = container;
			ResourceNodeLocationCanon = resourceNodeLocationCanon;
            RestrictionCanon          = restrictionCanon;

            signals.ResourceNodeBeingDestroyedSignal.Subscribe(OnNodeBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IResourceNodeFactory

		public bool CanBuildNode(IHexCell location, IResourceDefinition resource) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(resource == null) {
                throw new ArgumentNullException("resource");
            }
            return ResourceNodeLocationCanon.GetPossessionsOfOwner(location).Count() == 0
                && RestrictionCanon.IsResourceValidOnCell(resource, location);
        }

		public IResourceNode BuildNode(IHexCell location, IResourceDefinition resource, int copies) {
			if(!CanBuildNode(location, resource)) {
				throw new InvalidOperationException("CanBuildNode must return true on the argued location and resource");
            }

			var newNode = Container.InstantiateComponentOnNewGameObject<ResourceNode>();

			newNode.Resource = resource;
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
