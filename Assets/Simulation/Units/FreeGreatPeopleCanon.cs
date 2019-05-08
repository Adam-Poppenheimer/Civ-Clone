using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Units {

    public class FreeGreatPeopleCanon : IFreeGreatPeopleCanon {

        #region instance fields and properties

        #region from IFreeGreatPeopleCanon

        public bool IsActive {
            get { return _isActive; }
            set {
                if(_isActive != value) {
                    _isActive = value;

                    if(_isActive) {
                        SignalSubscriptions.Add(CivSignals .CivBeingDestroyed    .Subscribe(OnCivBeingDestroyed));
                        SignalSubscriptions.Add(CivSignals .CivDiscoveredTech    .Subscribe(OnCivDiscoveredTech));
                        SignalSubscriptions.Add(CivSignals .CivUnlockedPolicy    .Subscribe(OnCivUnlockedPolicy));
                        SignalSubscriptions.Add(CivSignals .CivUnlockedPolicyTree.Subscribe(OnCivUnlockedPolicyTree));
                        SignalSubscriptions.Add(CivSignals .CivFinishedPolicyTree.Subscribe(OnCivFinishedPolicyTree));
                        SignalSubscriptions.Add(CitySignals.GainedBuilding   .Subscribe(OnCityGainedBuilding));
                    }else {
                        SignalSubscriptions.ForEach(subscription => subscription.Dispose());
                        SignalSubscriptions.Clear();
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private Dictionary<ICivilization, int> GreatPeopleForCiv = new Dictionary<ICivilization, int>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private CivilizationSignals                           CivSignals;
        private CitySignals                                   CitySignals;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeGreatPeopleCanon(
            CivilizationSignals civSignals, CitySignals citySignals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            CivSignals          = civSignals;
            CitySignals         = citySignals;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IFreeGreatPeopleCanon

        public int GetFreeGreatPeopleForCiv(ICivilization civ) {
            int retval;

            GreatPeopleForCiv.TryGetValue(civ, out retval);

            return retval;
        }

        public void SetFreeGreatPeopleForCiv(ICivilization civ, int value) {
            GreatPeopleForCiv[civ] = value;
        }

        public void AddFreeGreatPersonToCiv(ICivilization civ) {
            int value;

            GreatPeopleForCiv.TryGetValue(civ, out value);

            value++;

            GreatPeopleForCiv[civ] = value;
        }

        public void RemoveFreeGreatPersonFromCiv(ICivilization civ) {
            int value;

            GreatPeopleForCiv.TryGetValue(civ, out value);

            value--;

            GreatPeopleForCiv[civ] = value;
        }

        public void ClearCiv(ICivilization civ) {
            GreatPeopleForCiv.Remove(civ);
        }

        public void Clear() {
            GreatPeopleForCiv.Clear();
        }

        #endregion

        private void OnCivBeingDestroyed(ICivilization civ) {
            ClearCiv(civ);
        }

        private void OnCivDiscoveredTech(UniRx.Tuple<ICivilization, ITechDefinition> data) {
            var civ  = data.Item1;
            var tech = data.Item2;

            SetFreeGreatPeopleForCiv(civ, GetFreeGreatPeopleForCiv(civ) + tech.FreeGreatPeopleProvided);
        }

        private void OnCivUnlockedPolicy(UniRx.Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            SetFreeGreatPeopleForCiv(civ, GetFreeGreatPeopleForCiv(civ) + policy.Bonuses.FreeGreatPeople);
        }

        private void OnCivUnlockedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            SetFreeGreatPeopleForCiv(civ, GetFreeGreatPeopleForCiv(civ) + tree.UnlockingBonuses.FreeGreatPeople);
        }

        private void OnCivFinishedPolicyTree(UniRx.Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            SetFreeGreatPeopleForCiv(civ, GetFreeGreatPeopleForCiv(civ) + tree.CompletionBonuses.FreeGreatPeople);
        }

        private void OnCityGainedBuilding(UniRx.Tuple<ICity, IBuilding> data) {
            var city     = data.Item1;
            var building = data.Item2;

            if(building.Template.FreeGreatPeople > 0) {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

                SetFreeGreatPeopleForCiv(cityOwner, GetFreeGreatPeopleForCiv(cityOwner) + building.Template.FreeGreatPeople);
            }
        }

        #endregion

    }

}
