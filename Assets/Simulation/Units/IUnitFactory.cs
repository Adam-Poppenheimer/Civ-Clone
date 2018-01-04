﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public interface IUnitFactory : IFactory<IHexCell, IUnitTemplate, ICivilization, IUnit> {

        #region properties

        IEnumerable<IUnit> AllUnits { get; }

        #endregion

    }

}