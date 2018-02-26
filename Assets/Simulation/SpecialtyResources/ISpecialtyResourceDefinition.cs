using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.SpecialtyResources {

    public interface ISpecialtyResourceDefinition {

        #region properties

        string name { get; }

        ResourceSummary BonusYieldBase { get; }

        ResourceSummary BonusYieldWhenImproved { get; }

        SpecialtyResourceType Type { get; }

        IImprovementTemplate Extractor { get; }

        Transform AppearancePrefab { get; }

        Sprite Icon { get; }

        #endregion

    }

}
