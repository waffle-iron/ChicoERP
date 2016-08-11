using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Chaps.Container
{
    public class DependencyResolver
    {
        protected LifeTimeMode LifeTimeMode { get; set; }
        protected Dictionary<Thread, Object> ThreadInstances { get; set; }
        protected Type To { get; set; }
        internal Type From { get; set; }
        protected Object Instance { get; set; }
        protected Container Parent { get; set; }

        internal DependencyResolver(Type from, Type to, Container parent)
        {
            LifeTimeMode = LifeTimeMode.Default;
            Parent = parent;
            To = to;
            From = from;
        }

        internal Object GetObject(bool isParameter)
        {
            LifeTimeMode temp = LifeTimeMode;
            if (temp == LifeTimeMode.Default)
            {
                temp = Parent.StandardLifeTimeMode;
            }

            switch (temp)
            {
                case LifeTimeMode.Transient:
                    return CreateNewInstance();
                case LifeTimeMode.Singelton:
                    if (Instance == null)
                    {
                        Instance = CreateNewInstance();
                    }
                    return Instance;
                case LifeTimeMode.Hierarchical:
                    if (isParameter)
                    {
                        return CreateNewInstance();
                    }
                    if (Instance == null)
                    {
                        Instance = CreateNewInstance();
                    }
                    return Instance;
                case LifeTimeMode.PerThread:
                    if (ThreadInstances == null)
                    {
                        ThreadInstances = new Dictionary<Thread, object>();
                    }
                    Object output;
                    if (ThreadInstances.TryGetValue(Thread.CurrentThread, out output)) { }
                    else
                    {
                        output = CreateNewInstance();
                        ThreadInstances.Add(Thread.CurrentThread, output);
                    }
                    return output;
                default:
                    return CreateNewInstance();
            }
        }

        private Object CreateNewInstance()
        {
            Object output;
            ConstructorInfo resolvedConstructor = ResolveConstructor(To);

            ParameterInfo[] parameters = resolvedConstructor.GetParameters();
            if (parameters.Length == 0)
            {
                output = resolvedConstructor.Invoke(null);
            }
            else
            {
                object[] parameterValues = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    parameterValues[i] = Parent.Resolve(parameters[i].ParameterType, true);

                output = resolvedConstructor.Invoke(parameterValues);
            }

            return output;
        }

        private static ConstructorInfo ResolveConstructor(Type result)
        {
            ConstructorInfo[] constructors = result.GetConstructors();

            ConstructorInfo constructorToInvoke = null;

            foreach (ConstructorInfo constructor in constructors)
            {
                List<Attribute> attributes = constructor.GetCustomAttributes().ToList<Attribute>();
                if (attributes.OfType<DIConstructor>().Any())
                {
                    constructorToInvoke = constructor;
                    break;
                }

                if (constructorToInvoke == null || constructor.GetParameters().Length >= constructorToInvoke.GetParameters().Length)
                {
                    constructorToInvoke = constructor;
                }

            }

            return constructorToInvoke;
        }

        #region FluentApi methodes

        /// <summary>
        /// Register a instance to resolve to. <code>LifeTimeMode</code> is set to Singelton.
        /// </summary>
        /// <exception cref="ContainerRegistrationException">Thrown if <paramref name="instance"/> isn't assignable from registered Type.</exception>
        /// <param name="instance">Instance to resolve to.</param>
        /// <returns>Resolver for further configuration.</returns>
        public DependencyResolver SingeltonInstance(object instance)
        {
            if (!(From.IsAssignableFrom(instance.GetType())))
                throw new ContainerRegistrationException(From, instance.GetType(), "Type '" + From.Name + "' is not assignable from object of type '" + instance.GetType().Name + "'. Make shure that '" + instance.GetType().Name + "' implemets '" + From.Name + "'.");
            Instance = instance;
            return this.LifeTime(LifeTimeMode.Singelton);
        }

        /// <summary>
        /// Set <see cref="UmzugManager.Chaps.Container.LifeTimeMode"/> for registered type.
        /// </summary>
        /// <param name="lifeTimeMode"><see cref="UmzugManager.Chaps.Container.LifeTimeMode"/> to set.</param>
        /// <returns>Resolver for further configuration.</returns>
        public DependencyResolver LifeTime(LifeTimeMode lifeTimeMode)
        {
            LifeTimeMode = lifeTimeMode;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// Enum to describe the lifecylce management for a dependency registratded in an <see cref="UmzugManager.Chaps.Container.Container"/> 
    /// </summary>
    public enum LifeTimeMode
    {
        /// <summary>
        /// In Singelton mode every resolvation returns the same instance of an object
        /// </summary>
        Singelton,
        /// <summary>
        /// In Transien mode every resolvation returns a new instance of an object.
        /// </summary>
        Transient,
        /// <summary>
        /// Hierarchical mode works similar to Singelton mode. The difference is that Hierarchical mode is injecting new instences into dependencies inside of the container.
        /// </summary>
        Hierarchical,
        /// <summary>
        /// In PerThread mode every Thread get its own instance of an object.
        /// </summary>
        PerThread,
        /// <summary>
        /// In Default mode the dependencie is resolved depending on <see cref="UmzugManager.Chaps.Container.Container.StandardLifeTimeMode"/>.
        /// When <see cref="UmzugManager.Chaps.Container.Container.StandardLifeTimeMode"/> is set to <see cref="UmzugManager.Chaps.Container.LifeTimeMode.Default"/> it is actualy set to <see cref="UmzugManager.Chaps.Container.LifeTimeMode.Transient"/>.
        /// </summary>
        Default
    }
}
