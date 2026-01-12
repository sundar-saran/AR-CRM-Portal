using CRM_Buddies_Task.Controllers;
using CRM_Buddies_Task.Models;
using CRM_Buddies_Task.Models.Interfaces;
using System;
using System.Web.Mvc;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Mvc5;

namespace CRM_Buddies_Task.App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register the concrete implementation for the interface
            container.RegisterType<IDbHelper, DbHelper>(new HierarchicalLifetimeManager());

            // Register controllers (optional but good practice)
            container.RegisterType<AccountController>(new InjectionConstructor());

            // Set the dependency resolver
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}