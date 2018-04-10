using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementFactory {

        #region properties

        IEnumerable<IImprovement> AllImprovements { get; }

        #endregion

        #region methods

        IImprovement BuildImprovement(IImprovementTemplate template, IHexCell location);

        IImprovement BuildImprovement(
            IImprovementTemplate template, IHexCell location, int workInvested,
            bool isConstructed, bool isPillaged
        );

        #endregion

    }

}
