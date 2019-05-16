using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public interface IResourceExchangeBuilder {

        #region methods

        void BuildResourceExchanges(ICivilization sender, ICivilization receiver, ExchangeSummary summary);

        #endregion

    }

}