﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class FreeBuildingsCanon : IFreeBuildingsCanon {

        #region instance fields and properties

        #region from IFreeBuildingsCanon

        public bool IsActive {
            get { return _isActive; }
            set {
                if(_isActive != value) {
                    _isActive = value;

                    if(_isActive) {
                        CivGainedCitySubscription     = CivSignals.CivGainedCity    .Subscribe(OnCivGainedCity);
                        CivDiscoveredTechSubscription = CivSignals.CivDiscoveredTech.Subscribe(OnCivDiscoveredTech);

                    }else {
                        CivGainedCitySubscription    .Dispose();
                        CivDiscoveredTechSubscription.Dispose();
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private DictionaryOfLists<ICivilization, IEnumerable<IBuildingTemplate>> PendingFreeBuildingsOfCiv =
            new DictionaryOfLists<ICivilization, IEnumerable<IBuildingTemplate>>();

        private IDisposable CivGainedCitySubscription;
        private IDisposable CivDiscoveredTechSubscription;



        
        
        private IFreeBuildingApplier                          FreeBuildingApplier;
        private CivilizationSignals                           CivSignals;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeBuildingsCanon(
            IFreeBuildingApplier freeBuildingApplier, CivilizationSignals civSignals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            FreeBuildingApplier = freeBuildingApplier;
            CivSignals          = civSignals;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IFreeBuildingCanon

        public void SubscribeFreeBuildingToCiv(
            IEnumerable<IBuildingTemplate> validTemplates, ICivilization civ
        ) {
            if(IsActive) {
                foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                    if(FreeBuildingApplier.CanApplyFreeBuildingToCity(validTemplates, city)) {
                        FreeBuildingApplier.ApplyFreeBuildingToCity(validTemplates, city);

                        return;
                    }
                }
            }

            PendingFreeBuildingsOfCiv.AddElementToList(civ, validTemplates);
        }

        public void RemoveFreeBuildingFromCiv(IEnumerable<IBuildingTemplate> validTemplates, ICivilization civ) {
            PendingFreeBuildingsOfCiv[civ].Remove(validTemplates);
        }

        public IEnumerable<IEnumerable<IBuildingTemplate>> GetFreeBuildingsForCiv(ICivilization civ) {
            return PendingFreeBuildingsOfCiv[civ];
        }

        public void ClearForCiv(ICivilization civ) {
            PendingFreeBuildingsOfCiv.RemoveList(civ);
        }

        public void Clear() {
            PendingFreeBuildingsOfCiv.Clear();
        }

        #endregion

        private void OnCivGainedCity(System.Tuple<ICivilization, ICity> data) {
            var civ  = data.Item1;
            var city = data.Item2;

            foreach(var freeBuilding in GetFreeBuildingsForCiv(civ).ToList()) {
                if(FreeBuildingApplier.CanApplyFreeBuildingToCity(freeBuilding, city)) {
                    FreeBuildingApplier.ApplyFreeBuildingToCity(freeBuilding, city);

                    RemoveFreeBuildingFromCiv(freeBuilding, civ);
                }
            }
        }

        private void OnCivDiscoveredTech(System.Tuple<ICivilization, ITechDefinition> data) {
            var civ  = data.Item1;
            var tech = data.Item2;

            foreach(var freeBuilding in GetFreeBuildingsForCiv(civ).ToList()) {

                if(tech.BuildingsEnabled.Intersect(freeBuilding).Any()) {
                    foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                        if(FreeBuildingApplier.CanApplyFreeBuildingToCity(freeBuilding, city)) {
                            FreeBuildingApplier.ApplyFreeBuildingToCity(freeBuilding, city);

                            RemoveFreeBuildingFromCiv(freeBuilding, civ);

                            break;
                        }
                    }
                }
            }
        }

        #endregion

    }

}
