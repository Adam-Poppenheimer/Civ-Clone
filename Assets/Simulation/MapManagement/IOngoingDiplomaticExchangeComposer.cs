using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.MapManagement {

    public interface IOngoingDiplomaticExchangeComposer {

        #region methods

        SerializableOngoingDiplomaticExchange ComposeOngoingExchange(IOngoingDiplomaticExchange ongoingExchange);

        IOngoingDiplomaticExchange DecomposeOngoingExchange(SerializableOngoingDiplomaticExchange ongoingData);

        #endregion

    }

}