using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Civilizations {

    public class FreeGoldenAgeResponder : IPlayModeSensitiveElement {

        #region instance fields and properties

        #region from IPlayModeSensitiveElement

        public bool IsActive {
            get { return _isActive; }
            set {
                if(_isActive != value) {
                    _isActive = value;

                    if(_isActive) {
                        SignalSubscriptions.Add(CivSignals.CivUnlockedPolicy    .Subscribe(OnCivUnlockedPolicy));
                        SignalSubscriptions.Add(CivSignals.CivUnlockedPolicyTree.Subscribe(OnCivUnlockedPolicyTree));
                        SignalSubscriptions.Add(CivSignals.CivFinishedPolicyTree.Subscribe(OnCivFinishedPolicyTree));

                        SignalSubscriptions.Add(CitySignals.GainedBuilding.Subscribe(OnCityGainedBuilding));
                    }else {
                        SignalSubscriptions.ForEach(subscription => subscription.Dispose());

                        SignalSubscriptions.Clear();
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IGoldenAgeCanon                               GoldenAgeCanon;
        private CivilizationSignals                           CivSignals;
        private CitySignals                                   CitySignals;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeGoldenAgeResponder(
            IGoldenAgeCanon goldenAgeCanon, CivilizationSignals civSignals, CitySignals citySignals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            GoldenAgeCanon      = goldenAgeCanon;
            CivSignals          = civSignals;
            CitySignals         = citySignals;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        private void OnCivUnlockedPolicy(Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            if(policy.Bonuses.StartsGoldenAge) {
                TryStartGoldenAge(civ);
            }
        }

        private void OnCivUnlockedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            if(tree.UnlockingBonuses.StartsGoldenAge) {
                TryStartGoldenAge(civ);
            }
        }

        private void OnCivFinishedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            if(tree.CompletionBonuses.StartsGoldenAge) {
                TryStartGoldenAge(civ);
            }
        }

        private void OnCityGainedBuilding(Tuple<ICity, IBuilding> data) {
            var city     = data.Item1;
            var building = data.Item2;

            if(building.Template.StartsGoldenAge) {
                TryStartGoldenAge(CityPossessionCanon.GetOwnerOfPossession(city));
            }
        }

        private void TryStartGoldenAge(ICivilization civ) {
            int goldenAgeDuration = GoldenAgeCanon.GetGoldenAgeLengthForCiv(civ);

            if(GoldenAgeCanon.IsCivInGoldenAge(civ)) {
                GoldenAgeCanon.ChangeTurnsOfGoldenAgeForCiv(civ, goldenAgeDuration);
            }else {
                GoldenAgeCanon.StartGoldenAgeForCiv(civ, goldenAgeDuration);
            }
        }

        #endregion

    }

}
