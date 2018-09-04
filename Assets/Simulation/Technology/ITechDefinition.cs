using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.MapResources;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Technology {

    public interface ITechDefinition {

        #region properties

        string Name { get; }

        int Cost { get; }

        Sprite Icon { get; }

        TechnologyEra Era { get; }

        int TechTableRow    { get; }
        int TechTableColumn { get; }

        IEnumerable<ITechDefinition> Prerequisites { get; }

        IEnumerable<IBuildingTemplate> BuildingsEnabled { get; }

        IEnumerable<IUnitTemplate> UnitsEnabled { get; }

        IEnumerable<IAbilityDefinition> AbilitiesEnabled { get; }

        IEnumerable<IImprovementModificationData> ImprovementYieldModifications { get; }

        IEnumerable<IResourceDefinition> RevealedResources { get; }

        IEnumerable<IPolicyTreeDefinition> PolicyTreesEnabled { get; }

        #endregion

    }

}
