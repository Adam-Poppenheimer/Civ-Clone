using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources{

    public class ResourceNodeFactory : IResourceNodeFactory {

        #region instance fields and properties

		private DiContainer Container;

		private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;

        #endregion

        #region constructors

		[Inject]
		public ResourceNodeFactory(DiContainer container,
			IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon
		){
			Container = container;
			ResourceNodeLocationCanon = resourceNodeLocationCanon;
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

			var appearanceElement = Container.InstantiatePrefab(definition.AppearancePrefab);

			appearanceElement.transform.SetParent(newNode.transform, false);

			ResourceNodeLocationCanon.ChangeOwnerOfPossession(newNode, location);

			return newNode;
        }

        #endregion

        #endregion
        
    }

}
