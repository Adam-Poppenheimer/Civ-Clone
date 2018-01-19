﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    public interface ITechDefinition {

        #region properties

        string Name { get; }

        int Cost { get; }

        Vector2 TechScreenPosition { get; }

        IEnumerable<ITechDefinition> Prerequisites { get; }

        IEnumerable<IBuildingTemplate> BuildingsEnabled { get; }

        IEnumerable<IUnitTemplate> UnitsEnabled { get; }

        IEnumerable<IImprovementTemplate> ImprovementsEnabled { get; }

        #endregion

    }

}
