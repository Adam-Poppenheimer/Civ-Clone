using System;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapManagement {

    public interface ISocialPolicyComposer {

        #region methods

        void ClearPolicyRuntime();

        SerializableSocialPolicyData ComposePoliciesFromCiv(ICivilization civ);

        void DecomposePoliciesIntoCiv(SerializableSocialPolicyData policyData, ICivilization civ);

        #endregion

    }

}