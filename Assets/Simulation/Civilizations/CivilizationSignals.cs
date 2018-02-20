﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.Simulation.Civilizations {

    public class CivilizationSignals {

        #region instance fields and properties

        public ISubject<ICivilization> CivilizationBeingDestroyedSignal { get; private set; }

        #endregion

        #region constructors

        public CivilizationSignals() {
            CivilizationBeingDestroyedSignal = new Subject<ICivilization>();
        }

        #endregion

    }

}
