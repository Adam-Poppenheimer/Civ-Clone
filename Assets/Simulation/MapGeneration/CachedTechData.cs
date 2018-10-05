using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public class CachedTechData {

        public HashSet<IResourceDefinition>          VisibleResources;
        public HashSet<IImprovementTemplate>         AvailableImprovements;
        public HashSet<IBuildingTemplate>            AvailableBuildings;
        public HashSet<IImprovementModificationData> ImprovementModifications;

    }

}

