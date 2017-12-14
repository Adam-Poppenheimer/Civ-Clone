using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementFactory : IFactory<IImprovementTemplate, IMapTile, IImprovement> {
    }

}
