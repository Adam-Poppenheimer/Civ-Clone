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

        public ISubject<ICivilization> NewCivilizationCreated { get; private set; }
        public ISubject<ICivilization> CivBeingDestroyed      { get; private set; }

        public ISubject<Tuple<ICivilization, ICity>> CivGainedCity { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLosingCity { get; private set; }
        public ISubject<Tuple<ICivilization, ICity>> CivLostCity   { get; private set; }

        public ISubject<Tuple<ICivilization, IUnit>> CivGainedUnit { get; private set; }
        public ISubject<Tuple<ICivilization, IUnit>> CivLosingUnit { get; private set; }
        public ISubject<Tuple<ICivilization, IUnit>> CivLostUnit   { get; private set; }

        public ISubject<ResourceTransfer> ResourceTransferCancelled { get; private set; }

        public ISubject<ICivilization> CivSelected { get; private set; }

        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivUnlockedPolicy { get; private set; }
        public ISubject<Tuple<ICivilization, ISocialPolicyDefinition>> CivLockedPolicy   { get; private set; }

        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivUnlockedPolicyTree { get; private set; }
        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivLockedPolicyTree   { get; private set; }

        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivFinishedPolicyTree   { get; private set; }
        public ISubject<Tuple<ICivilization, IPolicyTreeDefinition>> CivUnfinishedPolicyTree { get; private set; }

        public ISubject<ICivilization> CivDefeated { get; private set; }

        public ISubject<Tuple<ICivilization, ITechDefinition>> CivDiscoveredTech   { get; private set; }
        public ISubject<Tuple<ICivilization, ITechDefinition>> CivUndiscoveredTech { get; private set; }

        public ISubject<GreatPersonBirthData> GreatPersonBorn { get; private set; }

        public ISubject<ICivilization> CivEnteredGoldenAge { get; private set; }
        public ISubject<ICivilization> CivLeftGoldenAge    { get; private set; }

        #endregion

        #region constructors

        public CivilizationSignals(){
            NewCivilizationCreated = new Subject<ICivilization>();
            CivBeingDestroyed      = new Subject<ICivilization>();

            CivGainedCity = new Subject<Tuple<ICivilization, ICity>>();
            CivLosingCity = new Subject<Tuple<ICivilization, ICity>>();
            CivLostCity   = new Subject<Tuple<ICivilization, ICity>>();

            CivGainedUnit = new Subject<Tuple<ICivilization, IUnit>>();
            CivLosingUnit = new Subject<Tuple<ICivilization, IUnit>>();
            CivLostUnit   = new Subject<Tuple<ICivilization, IUnit>>();

            ResourceTransferCancelled = new Subject<ResourceTransfer>();

            CivSelected = new Subject<ICivilization>();

            CivUnlockedPolicy = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();
            CivLockedPolicy   = new Subject<Tuple<ICivilization, ISocialPolicyDefinition>>();

            CivUnlockedPolicyTree = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();
            CivLockedPolicyTree   = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();

            CivFinishedPolicyTree   = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();
            CivUnfinishedPolicyTree = new Subject<Tuple<ICivilization, IPolicyTreeDefinition>>();

            CivDefeated = new Subject<ICivilization>();

            CivDiscoveredTech   = new Subject<Tuple<ICivilization, ITechDefinition>>();
            CivUndiscoveredTech = new Subject<Tuple<ICivilization, ITechDefinition>>();

            GreatPersonBorn = new Subject<GreatPersonBirthData>();

            CivEnteredGoldenAge = new Subject<ICivilization>();
            CivLeftGoldenAge    = new Subject<ICivilization>();
        }

        #endregion

    }

}
