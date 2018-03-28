using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class ResourceComposer {

        #region instance fields and properties

        private IResourceNodeFactory                             NodeFactory;
        private IHexGrid                                         Grid;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IEnumerable<ISpecialtyResourceDefinition>        AvailableSpecialtyResources;

        #endregion

        #region constructors

        [Inject]
        public ResourceComposer(IResourceNodeFactory nodeFactory, IHexGrid grid,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            [Inject(Id = "Available Specialty Resources")] IEnumerable<ISpecialtyResourceDefinition> availableSpecialtyResources
        ){
            NodeFactory                 = nodeFactory;
            Grid                        = grid;
            NodeLocationCanon           = nodeLocationCanon;
            AvailableSpecialtyResources = availableSpecialtyResources;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var node in new List<IResourceNode>(NodeFactory.AllNodes)) {
                GameObject.Destroy(node.gameObject);
            }
        }

        public void ComposeResources(SerializableMapData mapData) {
            mapData.ResourceNodes = new List<SerializableResourceNodeData>();

            foreach(var node in NodeFactory.AllNodes) {
                var nodeLocation = NodeLocationCanon.GetOwnerOfPossession(node);

                var newNodeData = new SerializableResourceNodeData() {
                    Location = nodeLocation.Coordinates,
                    Resource = node.Resource.name,
                    Copies   = node.Copies
                };

                mapData.ResourceNodes.Add(newNodeData);
            }
        }

        public void DecomposeResources(SerializableMapData mapData) {
            foreach(var nodeData in mapData.ResourceNodes) {
                var nodeLocation = Grid.GetCellAtCoordinates(nodeData.Location);
                var nodeResource = AvailableSpecialtyResources.Where(
                    resource => resource.name.Equals(nodeData.Resource)
                ).First();

                NodeFactory.BuildNode(nodeLocation, nodeResource, nodeData.Copies);
            }
        }

        #endregion

    }

}
