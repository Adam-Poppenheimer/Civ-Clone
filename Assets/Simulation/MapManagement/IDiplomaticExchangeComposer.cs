using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.MapManagement {

    public interface IDiplomaticExchangeComposer {

        #region methods

        SerializableDiplomaticExchangeData ComposeExchange(IDiplomaticExchange exchange);

        IDiplomaticExchange DecomposeExchange(SerializableDiplomaticExchangeData exchangeData);

        #endregion

    }

}