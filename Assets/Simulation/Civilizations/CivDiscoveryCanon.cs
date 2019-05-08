using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.Civilizations {

    public class CivDiscoveryCanon : ICivDiscoveryCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, HashSet<ICivilization>> CivsDiscoveredByCiv =
            new Dictionary<ICivilization, HashSet<ICivilization>>();


        #endregion

        #region constructors

        [Inject]
        public CivDiscoveryCanon(CivilizationSignals civSignals) {
            civSignals.CivBeingDestroyed.Subscribe(OnCivBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from ICivDiscoveryCanon

        public IEnumerable<ICivilization> GetCivsDiscoveredByCiv(ICivilization civ) {
            HashSet<ICivilization> retval;

            if(!CivsDiscoveredByCiv.TryGetValue(civ, out retval)) {
                retval = new HashSet<ICivilization>();

                CivsDiscoveredByCiv[civ] = retval;
            }

            return retval;
        }

        public bool HaveCivsDiscoveredEachOther(ICivilization civOne, ICivilization civTwo) {
            return GetCivsDiscoveredByCiv(civOne).Contains(civTwo);
        }

        public bool CanEstablishDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo) {
            return (civOne != civTwo) && !GetCivsDiscoveredByCiv(civOne).Contains(civTwo);
        }

        public void EstablishDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo) {
            if(!CanEstablishDiscoveryBetweenCivs(civOne, civTwo)) {
                throw new InvalidOperationException("CanEstablishDiscoveryBetweenCivs must return true on the arguments");
            }

            AddValueToHashSet(civOne, civTwo);
            AddValueToHashSet(civTwo, civOne);
        }

        public bool CanRevokeDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo) {
            return GetCivsDiscoveredByCiv(civOne).Contains(civTwo);
        }

        public void RevokeDiscoveryBetweenCivs(ICivilization civOne, ICivilization civTwo) {
            if(!CanRevokeDiscoveryBetweenCivs(civOne, civTwo)) {
                throw new InvalidOperationException("CanRevokeDiscoveryBetweenCivs must return true on the arguments");
            }

            RemoveValueFromHashSet(civOne, civTwo);
            RemoveValueFromHashSet(civTwo, civOne);
        }

        public List<UniRx.Tuple<ICivilization, ICivilization>> GetDiscoveryPairs() {
            var retval = new List<UniRx.Tuple<ICivilization, ICivilization>>();

            foreach(var firstCiv in CivsDiscoveredByCiv.Keys) {
                foreach(var secondCiv in CivsDiscoveredByCiv[firstCiv]) {
                    if(!retval.Any(pair => IsPairOfCivs(pair, firstCiv, secondCiv))) {
                        retval.Add(new UniRx.Tuple<ICivilization, ICivilization>(firstCiv, secondCiv));
                    }
                }
            }

            return retval;
        }

        public void Clear() {
            CivsDiscoveredByCiv.Clear();
        }

        #endregion

        private bool IsPairOfCivs(UniRx.Tuple<ICivilization, ICivilization> pair, ICivilization firstCiv, ICivilization secondCiv) {
            return (pair.Item1 == firstCiv  && pair.Item2 == secondCiv)
                || (pair.Item1 == secondCiv && pair.Item2 == firstCiv);
        }

        private void AddValueToHashSet(ICivilization key, ICivilization newValue) {
            HashSet<ICivilization> set;

            if(!CivsDiscoveredByCiv.TryGetValue(key, out set)) {
                set = new HashSet<ICivilization>();
                CivsDiscoveredByCiv[key] = set;
            }

            set.Add(newValue);
        }

        private void RemoveValueFromHashSet(ICivilization key, ICivilization oldValue) {
            HashSet<ICivilization> set;

            if(CivsDiscoveredByCiv.TryGetValue(key, out set)) {
                set.Remove(oldValue);
            }

        }

        private void OnCivBeingDestroyed(ICivilization civ) {
            CivsDiscoveredByCiv.Remove(civ);

            foreach(var discoveredCivs in CivsDiscoveredByCiv.Values) {
                discoveredCivs.Remove(civ);
            }
        }

        #endregion
        
    }

}
