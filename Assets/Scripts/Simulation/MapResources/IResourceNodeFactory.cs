using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public interface IResourceNodeFactory {

        #region properties

        IEnumerable<IResourceNode> AllNodes { get; }

        #endregion

        #region methods

        bool CanBuildNode(IHexCell location, IResourceDefinition definition);

        IResourceNode BuildNode(IHexCell location, IResourceDefinition definition, int copies);

        #endregion

    }

}
