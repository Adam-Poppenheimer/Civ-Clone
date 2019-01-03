using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Players {

    [CreateAssetMenu(menuName = "Civ Clone/Players/Player Config")]
    public class PlayerConfig : ScriptableObject, IPlayerConfig {

        #region instance fields and properties

        #region from IPlayerConfig

        public IPlayerBrain HumanBrain     { get; private set; }
        public IPlayerBrain BarbarianBrain { get; private set; }

        #endregion

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Human Brain"    )] IPlayerBrain humanBrain,
            [Inject(Id = "Barbarian Brain")] IPlayerBrain barbarianBrain
        ) {
            HumanBrain     = humanBrain;
            BarbarianBrain = barbarianBrain;
        }

        #endregion

    }

}
