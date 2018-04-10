using System;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Performs the tasks that must occur at the beginning and end of turns 
    /// for various classes.
    /// </summary>
    public interface IRoundExecuter {

        #region methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="city"></param>
        void BeginRoundOnCity(ICity city);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="city"></param>
        void EndRoundOnCity  (ICity city);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="civilization"></param>
        void BeginRoundOnCivilization(ICivilization civilization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="civilization"></param>
        void EndRoundOnCivilization  (ICivilization civilization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        void BeginRoundOnUnit(IUnit unit);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        void EndRoundOnUnit  (IUnit unit);

        #endregion

    }

}