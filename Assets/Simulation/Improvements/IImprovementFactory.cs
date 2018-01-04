using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementFactory : IFactory<IImprovementTemplate, IHexCell, IImprovement> {
    }

}
