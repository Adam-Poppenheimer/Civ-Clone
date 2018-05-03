using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.MapManagement {

    public interface IOngoingDealComposer {

        #region methods

        SerializableOngoingDealData ComposeOngoingDeal(IOngoingDeal ongoingDeal);

        IOngoingDeal DecomposeOngoingDeal(SerializableOngoingDealData ongoingDealData);

        #endregion

    }

}