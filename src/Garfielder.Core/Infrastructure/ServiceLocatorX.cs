﻿using System;
using Microsoft.Practices.ServiceLocation;

namespace Garfielder.Core.Infrastructure
{
    /// <summary>
    /// This is a helper for accessing dependencies via the Common Service Locator (CSL).  But while
    /// the CSL will throw object reference errors if used before initialization, this will inform
    /// you of what the problem is."
    /// </summary>
    public static class ServiceLocatorX<TDependency>
    {
        public static TDependency GetService()
        {
            TDependency service;

            try
            {
                service = (TDependency)ServiceLocator.Current.GetService(typeof(TDependency));
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("ServiceLocator has not been initialized; " +
                        "I was trying to retrieve " + typeof(TDependency).ToString());
            }
            catch (ActivationException)
            {
                throw new ActivationException("The needed dependency of type " + typeof(TDependency).Name +
                        " could not be located with the ServiceLocator. You'll need to register it with " +
                        "the Common Service Locator (CSL) via your IoC's CSL adapter.");
            }

            return service;
        }
        public static TDependency GetInstance(string key)
        {
            TDependency service;

            try
            {
                service = (TDependency)ServiceLocator.Current.GetInstance<TDependency>(key);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("ServiceLocator has not been initialized; " +
                        "I was trying to retrieve " + typeof(TDependency).ToString());
            }
            catch (ActivationException)
            {
                throw new ActivationException("The needed dependency of type " + typeof(TDependency).Name +
                        " could not be located with the ServiceLocator. You'll need to register it with " +
                        "the Common Service Locator (CSL) via your IoC's CSL adapter.");
            }

            return service;
        }
    }

}
