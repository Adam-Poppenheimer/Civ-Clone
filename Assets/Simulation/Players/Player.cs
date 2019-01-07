using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Players {

    public class Player : IPlayer {

        #region instance fields and properties

        #region from IPlayer

        public string Name {
            get { return ControlledCiv.Template.Name; }
        }

        public ICivilization ControlledCiv { get; private set; }
        public IPlayerBrain  Brain         { get; private set; }

        #endregion

        #endregion

        #region constructors

        public Player(ICivilization controlledCiv, IPlayerBrain brain) {
            ControlledCiv = controlledCiv;
            Brain         = brain;
        }

        #endregion

        #region instance methods

        #region from IPlayer

        public void PassControl(Action controlRelinquisher) {
            Brain.RefreshAnalysis();
            Brain.ExecuteTurn(controlRelinquisher);
        }

        public void Clear() {
            Brain.Clear();
        }

        #endregion

        #endregion
        
    }

}
