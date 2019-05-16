using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Players {

    public class BrainPile : IBrainPile {

        #region instance fields and properties

        #region from IBrainPile

        public IPlayerBrain HumanBrain     { get; private set; }
        public IPlayerBrain BarbarianBrain { get; private set; }

        public IEnumerable<IPlayerBrain> AllBrains {
            get { return _allBrains; }
        }
        private List<IPlayerBrain> _allBrains;

        #endregion

        #endregion

        #region constructors

        [Inject]
        public BrainPile(            
            [Inject(Id = "Human Brain")] IPlayerBrain humanBrain,
            [Inject(Id = "Barbarian Brain")] IPlayerBrain barbarianBrain
        ) {
            HumanBrain     = humanBrain;
            BarbarianBrain = barbarianBrain;

            _allBrains = new List<IPlayerBrain>() {
                humanBrain, barbarianBrain
            };
        }

        #endregion

    }

}
