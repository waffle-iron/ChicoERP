using System;
using System.Collections.Generic;
using System.Linq;

namespace Chaps.Container
{
    /// <summary>
    /// Simple IoC container.
    /// </summary>
    public class Container
    {
        #region Properties
        internal List<DependencyResolver> DependencyList { get; set; }

        private LifeTimeMode standardLifeTimeMode;
        /// <summary>
        /// Default <see cref="LifeTimeMode"/> for the container.
        /// </summary>
        public LifeTimeMode StandardLifeTimeMode
        {
            get { return standardLifeTimeMode; }
            set
            {
                if (value == LifeTimeMode.Default)
                    standardLifeTimeMode = LifeTimeMode.Transient;
                else
                    standardLifeTimeMode = value;
            }
        }

        private Container _default;

        /// <summary>
        /// Default <see cref="Container"/> for static and global use.
        /// </summary>
        public Container Default { get { return _default ?? (new Container()); } }
        #endregion

        #region Constructors

        /// <summary>
        /// Initialises a new IOC container with default settings.
        /// </summary>
        /// <remarks>The standard <see cref="LifeTimeMode"/> is <see cref="LifeTimeMode.Transient"/></remarks>
        public Container() : this(LifeTimeMode.Transient) { }

        /// <summary>
        /// Initialises a new IOC container with a standard <see cref="LifeTimeMode"/>.
        /// </summary>
        /// <param name="standardLifeTimeMode">Standard <see cref="LifeTimeMode"/> for dependencies set to <see cref="LifeTimeMode.Default"/>.</param>
        public Container(LifeTimeMode standardLifeTimeMode)
        {
            DependencyList = new List<DependencyResolver>();
            StandardLifeTimeMode = standardLifeTimeMode;
        }

        #endregion

        #region Methodes

        /// <summary>
        /// Methode to register a new type genericly.
        /// </summary>
        /// <exception cref="ContainerRegistrationException">Thrown if types aren't assignable to each other or if type is already registered.</exception>
        /// <typeparam name="TFrom">Type to resolve from.</typeparam>
        /// <typeparam name="TTo">Type to resolve to.</typeparam>
        /// <returns>Resolver for further configuration.</returns>
        public DependencyResolver RegisterType<TFrom, TTo>() where TTo : class where TFrom : class
        {
            return RegisterType(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// Methode to register a new type by parameter.
        /// </summary>
        /// <exception cref="ContainerRegistrationException">Thrown if types aren't assignable to each other or if type is already registered.</exception>
        /// <param name="from">Type to resolve from.</param>
        /// <param name="to">Type to resolve to.</param>
        /// <returns>Resolver for further configuration.</returns>
        public DependencyResolver RegisterType(Type from, Type to)
        {
            if (!(from.IsAssignableFrom(to)))
                throw new ContainerRegistrationException(from, to, "Type '" + from.Name + "' is not assignable from type '" + to.Name + "'. Make shure that '" + to.Name + "' implemets '" + from.Name + "'.");

            lock (DependencyList)
            {
                if (DependencyList.Where(p => p.From == from).Count() > 0)
                    throw new ContainerRegistrationException(from, to, "Type '" + from.Name + "' is already registered");
                DependencyResolver resolver = new DependencyResolver(from, to, this);
                DependencyList.Add(resolver);
                return resolver;
            }
        }

        /// <summary>
        /// Methode to register a new type with a given instance. The <see cref="LifeTimeMode"/> is automaticly set to Singelton.
        /// </summary>
        /// <exception cref="ContainerRegistrationException">Thrown if <code>to</code> isn't assignable from <code>TInt</code> or if type is already registered.</exception>
        /// <typeparam name="TInt">Type to resolve from.</typeparam>
        /// <param name="To">Object to resolve to.</param>
        /// <returns>Resolver for further configuration.</returns>
        public DependencyResolver RegisterInstance<TInt>(Object To) where TInt : class
        {
            return RegisterType(typeof(TInt), To.GetType()).SingeltonInstance(To);
        }

        /// <summary>
        /// Methode to resolve a type to the corosponding object genericly.
        /// </summary>
        /// <typeparam name="TFrom">Type to resolve.</typeparam>
        /// <returns>Resolved object. <code>null</code> if the container couldn't resolve the type.</returns>
        public TFrom Resolve<TFrom>() where TFrom : class
        {
            return (TFrom)Resolve(typeof(TFrom));
        }


        internal Object Resolve(Type from, bool isParameter)
        {
            lock (DependencyList)
            {
                return DependencyList.Where(p => p.From == from).Count() != 0 ? DependencyList.Where(p => p.From == from).First().GetObject(isParameter) : null;
            }
        }

        /// <summary>
        /// Methode to resolve a type to the corosponding object.
        /// </summary>
        /// <param name="from">Type to resolve.</param>
        /// <returns>Resolved object. <code>null</code> if the container couldn't resolve the type.</returns>
        public Object Resolve(Type from)
        {
            return Resolve(from, false);

        }

        #endregion
    }

    /// <summary>
    /// Constructor marked by this attribute is prefered by the <code>Container</code>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public class DIConstructor : Attribute { }

    /// <summary>
    /// Exception is thrown in case of an error while registrating a new typ.
    /// </summary>
    public class ContainerRegistrationException : Exception
    {
        /// <summary>
        /// Type failed to register from.
        /// </summary>
        public Type FromType { get; protected set; }
        /// <summary>
        /// Type failed to register to.
        /// </summary>
        public Type ToType { get; protected set; }
        internal ContainerRegistrationException(Type fromType, Type toType) : this(fromType, toType, null, null)
        {

        }

        internal ContainerRegistrationException(Type fromType, Type toType, string message) : this(fromType, toType, message, null)
        {

        }

        internal ContainerRegistrationException(Type fromType, Type toType, string message, Exception inner) : base(message, inner)
        {
            FromType = fromType;
            ToType = toType;
        }
    }
}


