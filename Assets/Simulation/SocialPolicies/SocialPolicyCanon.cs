using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.SocialPolicies {

    public class SocialPolicyCanon : ISocialPolicyCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICivilization, ISocialPolicyDefinition> PoliciesUnlockedByCiv =
            new DictionaryOfLists<ICivilization, ISocialPolicyDefinition>();

        private DictionaryOfLists<ICivilization, IPolicyTreeDefinition> TreesUnlockedByCiv =
            new DictionaryOfLists<ICivilization, IPolicyTreeDefinition>();




        private ITechCanon                         TechCanon;
        private CivilizationSignals                CivSignals;
        private IEnumerable<IPolicyTreeDefinition> AvailablePolicyTrees;

        #endregion

        #region constructors

        [Inject]
        public SocialPolicyCanon(
            ITechCanon techCanon, CivilizationSignals civSignals,
            [Inject(Id = "Available Policy Trees")] IEnumerable<IPolicyTreeDefinition> availablePolicyTrees
        ) {
            TechCanon            = techCanon;
            CivSignals           = civSignals;
            AvailablePolicyTrees = availablePolicyTrees;
        }

        #endregion

        #region instance methods

        #region from ISocialPolicyCanon

        public IPolicyTreeDefinition GetTreeWithPolicy(ISocialPolicyDefinition policy) {
            return AvailablePolicyTrees.FirstOrDefault(
                tree => tree.Policies.Contains(policy)
            );
        }

        public IEnumerable<ISocialPolicyDefinition> GetPoliciesUnlockedFor(ICivilization civ) {
            return PoliciesUnlockedByCiv[civ];
        }

        public IEnumerable<IPolicyTreeDefinition> GetTreesUnlockedFor(ICivilization civ) {
            return TreesUnlockedByCiv[civ];
        }

        public IEnumerable<ISocialPolicyDefinition> GetPoliciesAvailableFor(ICivilization civ) {
            var unlockedPolicies = GetPoliciesUnlockedFor(civ);

            return TreesUnlockedByCiv[civ]
                .SelectMany(tree => tree.Policies)
                .Where(policy => policy.Prerequisites.All(prereq => unlockedPolicies.Contains(prereq)))
                .Except(unlockedPolicies);
        }

        public IEnumerable<IPolicyTreeDefinition> GetTreesAvailableFor(ICivilization civ) {
            return TechCanon.GetResearchedPolicyTrees(civ).Except(GetTreesUnlockedFor(civ));
        }

        public bool IsTreeCompletedByCiv(IPolicyTreeDefinition tree, ICivilization civ) {
            var unlockedPolicies = GetPoliciesUnlockedFor(civ);

            return GetTreesUnlockedFor(civ).Contains(tree)
                && tree.Policies.All(policy => unlockedPolicies.Contains(policy));
        }
        
        public void SetPolicyAsUnlockedForCiv(ISocialPolicyDefinition policy, ICivilization civ) {
            if(!GetPoliciesAvailableFor(civ).Contains(policy)) {
                throw new InvalidOperationException("Policy must be available to the given civ");

            }else if(GetPoliciesUnlockedFor(civ).Contains(policy)) {
                throw new InvalidOperationException("Policy already unlocked for the given civ");

            }else {
                PoliciesUnlockedByCiv[civ].Add(policy);

                var treeOfPolicy = GetTreeWithPolicy(policy);

                if(treeOfPolicy != null && IsTreeCompletedByCiv(treeOfPolicy, civ)) {
                    CivSignals.CivFinishedPolicyTree.OnNext(
                        new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, treeOfPolicy)
                    );
                }

                CivSignals.CivUnlockedPolicy.OnNext(
                    new UniRx.Tuple<ICivilization, ISocialPolicyDefinition>(civ, policy)
                );
            }
        }

        public void SetPolicyAsLockedForCiv(ISocialPolicyDefinition policy, ICivilization civ) {
            if(!GetPoliciesUnlockedFor(civ).Contains(policy)) {
                throw new InvalidOperationException("Policy already locked for the given civ");

            }else {
                var treeOfPolicy = GetTreeWithPolicy(policy);

                bool treeWasCompleted = treeOfPolicy != null && IsTreeCompletedByCiv(treeOfPolicy, civ);

                PoliciesUnlockedByCiv[civ].Remove(policy);

                CivSignals.CivLockedPolicy.OnNext(
                    new UniRx.Tuple<ICivilization, ISocialPolicyDefinition>(civ, policy)
                );

                if(treeWasCompleted) {
                    CivSignals.CivUnfinishedPolicyTree.OnNext(
                        new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, treeOfPolicy)
                    );
                }
            }
        }

        public void SetTreeAsUnlockedForCiv(IPolicyTreeDefinition tree, ICivilization civ) {
            if(!GetTreesAvailableFor(civ).Contains(tree)) {
                throw new InvalidOperationException("Tree must be available to the given civ");

            }else if(GetTreesUnlockedFor(civ).Contains(tree)) {
                throw new InvalidOperationException("Tree already unlocked for the given civ");

            }else {
                TreesUnlockedByCiv[civ].Add(tree);

                CivSignals.CivUnlockedPolicyTree.OnNext(
                    new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, tree)
                );
            }
        }

        public void SetTreeAsLockedForCiv(IPolicyTreeDefinition tree, ICivilization civ) {
            if(!GetTreesUnlockedFor(civ).Contains(tree)) {
                throw new InvalidOperationException("Tree must be unlocked for the given civ");

            }else {
                foreach(var policy in tree.Policies) {
                    SetPolicyAsLockedForCiv(policy, civ);
                }

                TreesUnlockedByCiv[civ].Remove(tree);

                CivSignals.CivLockedPolicyTree.OnNext(
                    new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, tree)
                );
            }
        }

        public void Clear() {
            PoliciesUnlockedByCiv.Clear();
            TreesUnlockedByCiv   .Clear();
        }

        public void OverrideUnlockedTreesForCiv(IEnumerable<IPolicyTreeDefinition> newUnlockedTrees, ICivilization civ) {
            TreesUnlockedByCiv[civ] = newUnlockedTrees.ToList();
        }

        public void OverrideUnlockedPoliciesForCiv(IEnumerable<ISocialPolicyDefinition> newUnlockedPolicies, ICivilization civ) {
            PoliciesUnlockedByCiv[civ] = newUnlockedPolicies.ToList();
        }

        public IEnumerable<ISocialPolicyBonusesData> GetPolicyBonusesForCiv(ICivilization civ) {
            var retval = GetPoliciesUnlockedFor(civ).Select(policy => policy.Bonuses);

            retval = retval.Concat(
                GetTreesUnlockedFor(civ).Select(tree => tree.UnlockingBonuses)
            );

            retval = retval.Concat(
                GetTreesUnlockedFor(civ).Where(tree => IsTreeCompletedByCiv(tree, civ))
                                        .Select(tree => tree.CompletionBonuses)
            );

            return retval;
        }

        #endregion

        #endregion

    }

}
