using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.MapManagement {

    public class DiplomacyComposer {

        #region instance fields and properties

        private IDiplomacyCore                            DiplomacyCore;
        private ICivilizationFactory                      CivFactory;
        private IPossessionRelationship<IHexCell, ICity>  CityLocationCanon;
        private IDiplomaticExchangeFactory                ExchangeFactory;
        private IHexGrid                                  Grid;
        private IWarCanon                                 WarCanon;
        private IEnumerable<ISpecialtyResourceDefinition> AvailableResources;

        #endregion

        #region constructors

        [Inject]
        public DiplomacyComposer(
            IDiplomacyCore diplomacyCore, ICivilizationFactory civFactory,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IDiplomaticExchangeFactory exchangeFactory, IHexGrid grid,
            IWarCanon warCanon,
            [Inject(Id = "Available Specialty Resources")] IEnumerable<ISpecialtyResourceDefinition> availableResources
        ) {
            DiplomacyCore      = diplomacyCore;
            CivFactory         = civFactory;
            CityLocationCanon  = cityLocationCanon;
            ExchangeFactory    = exchangeFactory;
            Grid               = grid;
            WarCanon           = warCanon;
            AvailableResources = availableResources;
        }

        #endregion

        #region instance methods

        public void ComposeDiplomacy(SerializableMapData mapData) {
            var diplomacyData = new SerializableDiplomacyData();

            foreach(var war in WarCanon.GetAllActiveWars()) {
                diplomacyData.ActiveWars.Add(
                    new Tuple<string, string>(war.Attacker.Name, war.Defender.Name)
                );
            }

            foreach(var fromCiv in CivFactory.AllCivilizations) {
                foreach(var proposalFrom in DiplomacyCore.GetProposalsSentFromCiv(fromCiv)) {
                    diplomacyData.ActiveProposals.Add(ComposeProposal(proposalFrom));
                }

                foreach(var ongoingDealFrom in DiplomacyCore.GetOngoingDealsSentFromCiv(fromCiv)) {
                    diplomacyData.ActiveOngoingDeals.Add(ComposeOngoingDeal(ongoingDealFrom));
                }
            }

            mapData.DiplomacyData = diplomacyData;
        }

        public void DecomposeDiplomacy(SerializableMapData mapData) {
            var diplomacyData = mapData.DiplomacyData;

            if(diplomacyData == null) {
                return;
            }

            var allCivs = CivFactory.AllCivilizations;

            foreach(var warData in diplomacyData.ActiveWars) {
                var attacker = allCivs.Where(civ => civ.Name.Equals(warData.Item1)).FirstOrDefault();

                if(attacker == null) {
                    throw new InvalidOperationException("Could not find a civ with name " + warData.Item1);
                }

                var defender = allCivs.Where(civ => civ.Name.Equals(warData.Item2)).FirstOrDefault();

                if(defender == null) {
                    throw new InvalidOperationException("Could not find a civ with name " + warData.Item2);
                }

                if(!WarCanon.CanDeclareWar(attacker, defender)) {
                    throw new InvalidOperationException(string.Format(
                        "Cannot declare the specified war between {0} and {1}",
                        attacker.Name, defender.Name
                    ));
                }

                WarCanon.DeclareWar(attacker, defender);
            }

            foreach(var proposalData in diplomacyData.ActiveProposals) {
                var proposal = DecomposeProposal(proposalData);

                DiplomacyCore.SendProposal(proposal);
            }

            foreach(var ongoingDealData in diplomacyData.ActiveOngoingDeals) {
                var ongoingDeal = DecomposeOngoingDeal(ongoingDealData);

                DiplomacyCore.SubscribeOngoingDeal(ongoingDeal);
            }
        }

        private SerializableProposalData ComposeProposal(IDiplomaticProposal proposal) {
            var retval = new SerializableProposalData();

            retval.Sender   = proposal.Sender.Name;
            retval.Receiver = proposal.Receiver.Name;

            foreach(var offer in proposal.OfferedBySender) {
                retval.OfferedBySender.Add(ComposeExchange(offer));
            }

            foreach(var demand in proposal.DemandedOfReceiver) {
                retval.DemandedOfReceiver.Add(ComposeExchange(demand));
            }

            foreach(var bilateral in proposal.BilateralExchanges) {
                retval.BilateralExchanges.Add(ComposeExchange(bilateral));
            }

            return retval;
        }

        private SerializableDiplomaticExchangeData ComposeExchange(IDiplomaticExchange exchange) {
            var retval = new SerializableDiplomaticExchangeData();

            retval.Type         = exchange.Type;
            retval.IntegerInput = exchange.IntegerInput;

            if(exchange.CityInput != null) {
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(exchange.CityInput);
                retval.CityInputLocation = cityLocation.Coordinates;
            }
            
            if(exchange.ResourceInput != null) {
                retval.ResourceInput = exchange.ResourceInput.name;
            }

            return retval;
        }

        private IDiplomaticProposal DecomposeProposal(SerializableProposalData proposalData) {
            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(proposalData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(proposalData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            var retval = new DiplomaticProposal(sender, receiver);

            foreach(var offerData in proposalData.OfferedBySender) {
                retval.AddAsOffer(DecomposeExchange(offerData));
            }

            foreach(var demandData in proposalData.DemandedOfReceiver) {
                retval.AddAsDemand(DecomposeExchange(demandData));
            }

            foreach(var bilateralData in proposalData.BilateralExchanges) {
                retval.AddAsBilateralExchange(DecomposeExchange(bilateralData));
            }

            return retval;
        }

        private IDiplomaticExchange DecomposeExchange(SerializableDiplomaticExchangeData exchangeData) {
            var retval = ExchangeFactory.BuildExchangeForType(exchangeData.Type);

            retval.IntegerInput = exchangeData.IntegerInput;

            if(exchangeData.CityInputLocation != null) {
                var cellAtCoords = Grid.GetCellAtCoordinates(exchangeData.CityInputLocation);

                if(cellAtCoords == null) {
                    throw new InvalidOperationException("Could not find a cell at the specified coordinates");
                }

                var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(cellAtCoords).FirstOrDefault();

                if(cityAtLocation == null) {
                    throw new InvalidOperationException("Could not find a city at the specified location");
                }

                retval.CityInput = cityAtLocation;
            }

            if(exchangeData.ResourceInput != null) {
                var resourceOfName = AvailableResources.Where(resource => resource.name.Equals(exchangeData.ResourceInput)).FirstOrDefault();

                if(resourceOfName == null) {
                    throw new InvalidOperationException("Could not find the resource of the specified name");
                }

                retval.ResourceInput = resourceOfName;
            }

            return retval;
        }

        private SerializableOngoingDeal ComposeOngoingDeal(IOngoingDeal ongoingDeal) {
            var retval = new SerializableOngoingDeal();

            retval.Sender   = ongoingDeal.Sender.Name;
            retval.Receiver = ongoingDeal.Receiver.Name;

            foreach(var offer in ongoingDeal.ExchangesFromSender) {
                retval.ExchangesFromSender.Add(ComposeOngoingExchange(offer));
            }

            foreach(var demand in ongoingDeal.ExchangesFromReceiver) {
                retval.ExchangesFromReceiver.Add(ComposeOngoingExchange(demand));
            }

            foreach(var bilateral in ongoingDeal.BilateralExchanges) {
                retval.BilateralExchanges.Add(ComposeOngoingExchange(bilateral));
            }

            return retval;
        }

        private SerializableOngoingDiplomaticExchange ComposeOngoingExchange(IOngoingDiplomaticExchange ongoingExchange) {
            var retval = new SerializableOngoingDiplomaticExchange();

            retval.Type          = ongoingExchange.Type;
            retval.Sender        = ongoingExchange.Sender       .Name;
            retval.Receiver      = ongoingExchange.Receiver     .Name;
            retval.ResourceInput = ongoingExchange.ResourceInput.name;
            retval.IntInput      = ongoingExchange.IntegerInput;

            return retval;
        }

        private IOngoingDeal DecomposeOngoingDeal(SerializableOngoingDeal ongoingDealData) {
            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingDealData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingDealData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            var fromSender   = new List<IOngoingDiplomaticExchange>();
            var fromReceiver = new List<IOngoingDiplomaticExchange>();
            var bilateral    = new List<IOngoingDiplomaticExchange>();

            foreach(var offerData in ongoingDealData.ExchangesFromSender) {
                fromSender.Add(DecomposeOngoingExchange(offerData));
            }

            foreach(var demandData in ongoingDealData.ExchangesFromReceiver) {
                fromReceiver.Add(DecomposeOngoingExchange(demandData));
            }

            foreach(var bilateralData in ongoingDealData.BilateralExchanges) {
                bilateral.Add(DecomposeOngoingExchange(bilateralData));
            }

            var retval = new OngoingDeal(sender, receiver, fromSender, fromReceiver, bilateral);

            retval.TurnsLeft = ongoingDealData.TurnsLeft;

            return retval;
        }

        private IOngoingDiplomaticExchange DecomposeOngoingExchange(SerializableOngoingDiplomaticExchange ongoingData) {
            var retval = ExchangeFactory.BuildOngoingExchangeForType(ongoingData.Type);

            var sender   = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingData.Sender))  .FirstOrDefault();
            var receiver = CivFactory.AllCivilizations.Where(civ => civ.Name.Equals(ongoingData.Receiver)).FirstOrDefault();

            if(sender == null) {
                throw new InvalidOperationException("Could not find a sender of the specified name");
            }

            if(receiver == null) {
                throw new InvalidOperationException("Could not find a receiver of the specified name");
            }

            retval.Sender   = sender;
            retval.Receiver = receiver;
            retval.IntegerInput = ongoingData.IntInput;

            if(ongoingData.ResourceInput != null) {
                var resourceInput = AvailableResources.Where(resource => resource.name.Equals(ongoingData.ResourceInput)).FirstOrDefault();

                if(resourceInput == null) {
                    throw new InvalidOperationException("Could not find a resource with the specified name");
                }

                retval.ResourceInput = resourceInput;
            }

            return retval;
        }

        #endregion

    }

}
