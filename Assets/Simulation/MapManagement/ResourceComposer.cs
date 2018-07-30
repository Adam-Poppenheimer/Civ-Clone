using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class ResourceComposer : IResourceComposer {

        #region instance fields and properties

        private IResourceNodeFactory                             NodeFactory;
        private IHexGrid                                         Grid;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IEnumerable<IResourceDefinition>                 AvailableResources;

        #endregion

        #region constructors

        [Inject]
        public ResourceComposer(IResourceNodeFactory nodeFactory, IHexGrid grid,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableSpecialtyResources
        ){
            NodeFactory                 = nodeFactory;
            Grid                        = grid;
            NodeLocationCanon           = nodeLocationCanon;
            AvailableResources = availableSpecialtyResources;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var node in new List<IResourceNode>(NodeFactory.AllNodes)) {
                NodeLocationCanon.ChangeOwnerOfPossession(node, null);
                node.Destroy();
            }
        }

        public void ComposeResources(SerializableMapData mapData) {
            mapData.ResourceNodes = new List<SerializableResourceNodeData>();

            foreach(var node in NodeFactory.AllNodes) {
                var nodeLocation = NodeLocationCanon.GetOwnerOfPossession(node);

                var newNodeData = new SerializableResourceNodeData() {
                    Location = nodeLocation.Coordinates,
                    ResourceName = node.Resource.name,
                    Copies   = node.Copies
                };

                mapData.ResourceNodes.Add(newNodeData);
            }
        }

        public void DecomposeResources(SerializableMapData mapData) {
            foreach(var nodeData in mapData.ResourceNodes) {
                var nodeLocation = Grid.GetCellAtCoordinates(nodeData.Location);
                var nodeResource = AvailableResources.Where(
                    resource => resource.name.Equals(nodeData.ResourceName)
                ).First();

                NodeFactory.BuildNode(nodeLocation, nodeResource, nodeData.Copies);
            }
        }

        #endregion

    }

}
