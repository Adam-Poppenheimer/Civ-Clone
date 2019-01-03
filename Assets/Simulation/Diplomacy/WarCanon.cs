using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class WarCanon : IWarCanon {

        #region instance fields and properties

        private List<WarData> ActiveWars = new List<WarData>();



        private ICivilizationFactory CivilizationFactory;

        #endregion

        #region constructors

        [Inject]
        public WarCanon(ICivilizationFactory civilizationFactory) {
            CivilizationFactory = civilizationFactory;
        }

        #endregion

        #region instance methods

        #region from IWarCanon

        public bool AreAtWar(ICivilization civOne, ICivilization civTwo) {
            if(civOne.Template.IsBarbaric || civTwo.Template.IsBarbaric) {
                return true;
            }else {
                return ActiveWars.Where(data => 
                    (data.Attacker == civOne && data.Defender == civTwo) ||
                    (data.Attacker == civTwo && data.Defender == civOne)
                ).Count() > 0;
            }
        }

        public bool AreAtPeace(ICivilization civOne, ICivilization civTwo) {
            return !AreAtWar(civOne, civTwo);
        }

        public IEnumerable<ICivilization> GetCivsAtWarWithCiv(ICivilization civ) {
            var retval = new HashSet<ICivilization>();

            foreach(var war in ActiveWars) {
                if(war.Attacker == civ) {
                    retval.Add(war.Defender);
                }else if(war.Defender == civ) {
                    retval.Add(war.Attacker);
                }
            }

            return retval;
        }

        public IEnumerable<ICivilization> GetCivsAtPeaceWithCiv(ICivilization civ) {
            return CivilizationFactory.AllCivilizations
                .Except(GetCivsAtWarWithCiv(civ))
                .Where(otherCiv => otherCiv != civ);
        }

        public bool CanDeclareWar(ICivilization attacker, ICivilization defender) {
            return !AreAtWar(attacker, defender) && attacker != defender;
        }

        public void DeclareWar(ICivilization attacker, ICivilization defender) {
            if(!CanDeclareWar(attacker, defender)) {
                throw new InvalidOperationException("CanDeclareWar must return true on the given arguments");
            }

            ActiveWars.Add(new WarData() { Attacker = attacker, Defender = defender });
        }

        public bool CanEstablishPeace(ICivilization civOne, ICivilization civTwo) {
            return !civOne.Template.IsBarbaric && !civTwo.Template.IsBarbaric && AreAtWar(civOne, civTwo);
        }

        public void EstablishPeace(ICivilization civOne, ICivilization civTwo) {
            if(!CanEstablishPeace(civOne, civTwo)) {
                throw new InvalidOperationException("CanEstablishPeace must return true on the given arguments");
            }

            ActiveWars.RemoveAll(war => 
                (war.Attacker == civOne && war.Defender == civTwo) ||
                (war.Defender == civOne && war.Attacker == civTwo)
            );
        }

        public IEnumerable<WarData> GetAllActiveWars() {
            return ActiveWars;
        }

        public void Clear() {
            ActiveWars.Clear();
        }

        #endregion

        #endregion

    }

}
