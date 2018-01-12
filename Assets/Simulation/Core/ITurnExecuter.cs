using System;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Core {

    /// <summary>
    /// Performs the tasks that must occur at the beginning and end of turns 
    /// for various classes.
    /// </summary>
    public interface ITurnExecuter {

        #region methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="city"></param>
        void BeginTurnOnCity(ICity city);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="city"></param>
        void EndTurnOnCity  (ICity city);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="civilization"></param>
        void BeginTurnOnCivilization(ICivilization civilization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="civilization"></param>
        void EndTurnOnCivilization  (ICivilization civilization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        void BeginTurnOnUnit(IUnit unit);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        void EndTurnOnUnit  (IUnit unit);

        #endregion

    }

}