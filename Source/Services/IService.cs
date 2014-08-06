using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomaSim.Services
{
    /// <summary>
    /// Interface for services that start up when the game starts up, and shut down when the user quits.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Called when the application starts up to start initializing the service.
        /// </summary>
        void Initialize ();

        /// <summary>
        /// Called when the application shuts down
        /// </summary>
        void Release ();
    }

    /// <summary>
    /// Interface for services that will also be updated on every frame
    /// </summary>
    public interface IUpdateService : IService
    {
        /// <summary>
        /// Called on every frame, from the MonoBehaviour.Update() function.
        /// </summary>
        void Update ();
    }
}
