using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Analytics
{
    /// <summary>
    /// Interface for all analytics providers
    /// </summary>
    public interface IAnalyticsProvider
    {
        #region Initialization 

        /// <summary>
        /// This function should be called right as the game starts up.
        /// It has a chance to initialize the provider and optionally send over startup stats.
        /// </summary>
        void Initialize ();

        /// <summary>
        /// This function should be called before the user quits the game.
        /// The provider may optionally flush any cached stats, and shut down.
        /// </summary>
        void Release ();

        /// <summary>
        /// Called on every frame. Whether anything needs to be done is up to implementation.
        /// </summary>
        void Update ();

        #endregion 

        #region Reporting API

        /// <summary>
        /// Logs an event with given name and arguments. The meaning of 
        /// the arguments is provider-specific. 
        /// </summary>
        /// <param name="arguments"></param>
        void LogEvent (params string[] arguments);

        #endregion
    }
}
