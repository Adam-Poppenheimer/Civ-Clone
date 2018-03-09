using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all building templates, which define what buildings are
    /// available for construction in various cities.
    /// </summary>
    public interface IBuildingTemplate {

        #region properties

        /// <summary>
        /// The name of the template.
        /// </summary>
        string name { get; }
        
        string Description { get; }

        Sprite Icon { get; }

        /// <summary>
        /// The production required to construct the template.
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// The maintenance, in gold per turn, of the building.
        /// </summary>
        int Maintenance { get; }

        /// <summary>
        /// The yield the building provides to its city just by existing.
        /// </summary>
        ResourceSummary StaticYield { get; }

        /// <summary>
        /// The yields of all of the slots the building provides to its city.
        /// </summary>
        ReadOnlyCollection<ResourceSummary> SlotYields { get; }

        /// <summary>
        /// The yield modifier the building provides to the civilization of the city it's in.
        /// </summary>
        ResourceSummary CivilizationYieldModifier { get; }

        /// <summary>
        /// The yield modifier the building provides to the city it's in.
        /// </summary>
        ResourceSummary CityYieldModifier { get; }

        int Health { get; }

        int Happiness { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        IEnumerable<IResourceYieldModificationData> ResourceYieldModifications { get; }

        #endregion

    }

}
