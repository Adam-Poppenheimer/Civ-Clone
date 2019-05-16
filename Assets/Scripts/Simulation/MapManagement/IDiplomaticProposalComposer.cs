using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.MapManagement {

    public interface IDiplomaticProposalComposer {

        #region methods

        SerializableProposalData ComposeProposal(IDiplomaticProposal proposal);

        IDiplomaticProposal DecomposeProposal(SerializableProposalData proposalData);

        #endregion

    }

}