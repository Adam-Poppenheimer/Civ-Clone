using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Units;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Civilizations {

    public class CivilizationSignals {

        #region instance fields and properties

        public ISubject<ICivilization> NewCivilizationCreatedSignal     { get; private set; }
        public ISubject<ICivilization> CivilizationBeingDestroyedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ICity>> CivGainedCitySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLosingCitySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLostCitySignal   { get; private set; }

        public ISubject<Tuple<ICivilization, IUnit>> CivGainedUnitSignal { get; private set; }
        public ISubject<Tuple<ICivilization, IUnit>> CivLosingUnitSignal { get; private set; }
        public ISubject<Tuple<ICivilization, IUnit>> CivLostUnitSignal   { get; private set; }

        public ISubject<ResourceTransfer> ResourceTransferCanceledSignal { get; private set; }

        public ISubject<ICivilization> CivSelectedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivUnlockedPolicySignal { get; private set; }
        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivLockedPolicySignal   { get; private set; }

        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivUnlockedPolicyTreeSignal { get; private set; }
        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivLockedPolicyTreeSignal   { get; private set; }

        public ISubject<ICivilization> CivDefeatedSignal { get; private set; }

        public ISubject<Tuple<ICivilization, ITechDefinition>> CivDiscoveredTechSignal { get; private set; }

        public ISubject<GreatPersonBirthData> GreatPersonBornSignal { get; private set; }

        #endregion

        #region constructors

        public CivilizationSignals(){
            NewCivilizationCreatedSignal     = new Subject<ICivilization>();
            CivilizationBeingDestroyedSignal = new Subject<ICivilization>();

            CivGainedCitySignal = new Subject<Tuple<ICivilization, ICity>>();
            CivLosingCitySignal = new Subject<Tuple<ICivilization, ICity>>();
            CivLostCitySignal   = new Subject<Tuple<ICivilization, ICity>>();

            CivGainedUnitSignal = new Subject<Tuple<ICivilization, IUnit>>();
            CivLosingUnitSignal = new Subject<Tuple<ICivilization, IUnit>>();
            CivLostUnitSignal   = new Subject<Tuple<ICivilization, IUnit>>();

            ResourceTransferCanceledSignal = new Subject<ResourceTransfer>();

            CivSelectedSignal = new Subject<ICivilization>();

            CivUnlockedPolicySignal = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();
            CivLockedPolicySignal   = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();

            CivUnlockedPolicyTreeSignal = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();
            CivLockedPolicyTreeSignal   = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();

            CivDefeatedSignal = new Subject<ICivilization>();

            CivDiscoveredTechSignal = new Subject<Tuple<ICivilization, ITechDefinition>>();

            GreatPersonBornSignal = new Subject<GreatPersonBirthData>();
        }

        #endregion

    }

}
