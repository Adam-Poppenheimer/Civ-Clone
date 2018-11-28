using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyCanon {

        #region methods

        IPolicyTreeDefinition GetTreeWithPolicy(ISocialPolicyDefinition policy);

        IEnumerable<ISocialPolicyDefinition> GetPoliciesUnlockedFor(ICivilization civ);        
        IEnumerable<IPolicyTreeDefinition>   GetTreesUnlockedFor   (ICivilization civ);

        IEnumerable<ISocialPolicyDefinition> GetPoliciesAvailableFor(ICivilization civ);
        IEnumerable<IPolicyTreeDefinition>   GetTreesAvailableFor   (ICivilization civ);

        bool IsTreeCompletedByCiv(IPolicyTreeDefinition tree, ICivilization civ);

        void SetPolicyAsUnlockedForCiv(ISocialPolicyDefinition policy, ICivilization civ);
        void SetPolicyAsLockedForCiv  (ISocialPolicyDefinition policy, ICivilization civ);

        void SetTreeAsUnlockedForCiv(IPolicyTreeDefinition tree, ICivilization civ);
        void SetTreeAsLockedForCiv  (IPolicyTreeDefinition tree, ICivilization civ);

        void Clear();

        void OverrideUnlockedTreesForCiv   (IEnumerable<IPolicyTreeDefinition>   newUnlockedTrees,    ICivilization civ);
        void OverrideUnlockedPoliciesForCiv(IEnumerable<ISocialPolicyDefinition> newUnlockedPolicies, ICivilization civ);

        IEnumerable<ISocialPolicyBonusesData> GetPolicyBonusesForCiv(ICivilization civ);

        #endregion

    }
}
