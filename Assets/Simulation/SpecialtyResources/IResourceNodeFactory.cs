using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public interface IResourceNodeFactory {

        #region properties

        IEnumerable<IResourceNode> AllNodes { get; }

        #endregion

        #region methods

        bool CanBuildNode(IHexCell location, ISpecialtyResourceDefinition definition);

        IResourceNode BuildNode(IHexCell location, ISpecialtyResourceDefinition definition, int copies);

        #endregion

    }

}
