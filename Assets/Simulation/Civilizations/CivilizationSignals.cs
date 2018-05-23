using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Civilizations {

    public class CivilizationSignals {

        #region instance fields and properties

        public ISubject<ICivilization> NewCivilizationCreatedSignal     { get; private set; }
        public ISubject<ICivilization> CivilizationBeingDestroyedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ICity>> CivGainedCitySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLosingCitySignal { get; private set; }

        public ISubject<ResourceTransfer> ResourceTransferCanceledSignal { get; private set; }

        public ISubject<ICivilization> CivSelectedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivUnlockedPolicySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivLockedPolicySignal   { get; private set; }

        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivUnlockedPolicyTreeSignal { get; private set; }
        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivLockedPolicyTreeSignal   { get; private set; }

        #endregion

        #region constructors

        public CivilizationSignals(){
            NewCivilizationCreatedSignal     = new Subject<ICivilization>();
            CivilizationBeingDestroyedSignal = new Subject<ICivilization>();

            CivGainedCitySignal = new Subject<Tuple<ICivilization, ICity>>();
            CivLosingCitySignal = new Subject<Tuple<ICivilization, ICity>>();

            ResourceTransferCanceledSignal = new Subject<ResourceTransfer>();

            CivSelectedSignal = new Subject<ICivilization>();

            CivUnlockedPolicySignal = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();
            CivLockedPolicySignal   = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();

            CivUnlockedPolicyTreeSignal = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();
            CivLockedPolicyTreeSignal   = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();
        }

        #endregion

    }

}
