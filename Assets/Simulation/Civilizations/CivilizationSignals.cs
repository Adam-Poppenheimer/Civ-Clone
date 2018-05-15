using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CivilizationSignals {

        #region instance fields and properties

        public ISubject<ICivilization> NewCivilizationCreatedSignal     { get; private set; }
        public ISubject<ICivilization> CivilizationBeingDestroyedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ICity>> CivGainedCitySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLosingCitySignal { get; private set; }

        public ISubject<ResourceTransfer> ResourceTransferCanceledSignal { get; private set; }

        public ISubject<ICivilization> CivSelectedSignal { get; private set; }
        

        #endregion

        #region constructors

        public CivilizationSignals(){
            NewCivilizationCreatedSignal     = new Subject<ICivilization>();
            CivilizationBeingDestroyedSignal = new Subject<ICivilization>();

            CivGainedCitySignal = new Subject<Tuple<ICivilization, ICity>>();
            CivLosingCitySignal = new Subject<Tuple<ICivilization, ICity>>();

            ResourceTransferCanceledSignal = new Subject<ResourceTransfer>();

            CivSelectedSignal = new Subject<ICivilization>();
        }

        #endregion

        #region instance methods

        

        #endregion

    }

}
