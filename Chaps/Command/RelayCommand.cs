using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chaps.Command
{

    /// <summary>
    /// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>
    /// The constructor deliberately prevents the use of value types.
    /// Because ICommand takes an object, having a value type for T would cause unexpected behavior when CanExecute(null) is called during XAML initialization for command bindings.
    /// Using default(T) was considered and rejected as a solution because the implementor would not be able to distinguish between a valid and defaulted values.
    /// <para/>
    /// Instead, callers should support a value type by using a nullable value type and checking the HasValue property before using the Value property.
    /// <example>
    ///     <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new RelayCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    /// 
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    /// </example>
    /// </remarks>
    public class RelayCommand<T> : RelayCommandBase
    {
        
        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><see cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> executeMethode): this(executeMethode, (o) => true){

        }

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <exception cref="ArgumentNullException">When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> ar <see langword="null" />.</exception>
        public RelayCommand(Action<T> executeMethode, Func<T, bool> canExecuteMethode) : base((o)=> executeMethode((T)o), (o)=>canExecuteMethode((T)o))
        {
            if (executeMethode == null || canExecuteMethode == null)
                throw new ArgumentNullException(nameof(executeMethode), Resources.RelayCommandDelegatesCannotBeNull);

            TypeInfo genericTypeInfo = typeof(T).GetTypeInfo();

            if (genericTypeInfo.IsValueType)
            {
                if((!genericTypeInfo.IsGenericType) || (!typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(genericTypeInfo.GetGenericTypeDefinition().GetTypeInfo())))
                {
                    throw new InvalidCastException(Resources.RelayCommandInvalidGenericPayloadType);
                }
            }
        }

        /// <summary>
        /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute((o) => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        public RelayCommand<T> ObserveCanExecute(Expression<Func<object, bool>> canExecuteExprtession)
        {
            ObserveCanExecuteInternal(canExecuteExprtession);
            return this;
        }

        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="TP">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        public RelayCommand<T> ObserveProperty<TP>(Expression<Func<TP>> propertyExpression)
        {
            ObservePropertyInternal(propertyExpression);
            return this;
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="RelayCommand{T}"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="RelayCommand{T}"/></returns>
        public static RelayCommand<T> FromAsyncHandler(Func<T, Task> executeMethod)
        {
            return new RelayCommand<T>(executeMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="RelayCommand{T}"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <returns>Constructed instance of <see cref="RelayCommand{T}"/></returns>
        public static RelayCommand<T> FromAsyncHandler(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            return new RelayCommand<T>(executeMethod, canExecuteMethod);
        }

        ///<summary>
        ///Determines if the command can execute by invoked the <see cref="Func{T,Bool}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command to determine if it can execute.</param>
        ///<returns>
        ///<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        ///</returns>
        public virtual bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        ///<summary>
        ///Executes the command and invokes the <see cref="Action{T}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command.</param>
        public virtual Task Execute(T parameter)
        {
            return base.Execute(parameter);
        }


        protected RelayCommand(Func<T, Task> executeMethod): this(executeMethod, (o) => true)
        {
        
        }

        protected RelayCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), Resources.RelayCommandDelegatesCannotBeNull);
        }
    }

    /// <summary>
    /// An <see cref="ICommand"/> whose delegates do not take any parameters for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    /// <see cref="RelayCommandBase"/>
    /// <see cref="RelayCommand{T}"/>
    public class RelayCommand : RelayCommandBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/> with the <see cref="Action"/> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        public RelayCommand(Action executeMethod): this(executeMethod, () => true)
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/> with the <see cref="Action"/> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{TResult}"/> to invoke when <see cref="ICommand.CanExecute"/> is called</param>
        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethode) :base((o)=> executeMethod(), (o) => canExecuteMethode())
        {
            if (executeMethod == null || canExecuteMethode == null)
                throw new ArgumentNullException(nameof(executeMethod), Resources.RelayCommandDelegatesCannotBeNull);
        }

        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        public RelayCommand ObservesProperty<T>(Expression<Func<T>> propertyExpression)
        {
            ObservePropertyInternal(propertyExpression);
            return this;
        }

        /// <summary>
        /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute((o) => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        public RelayCommand ObeserveCanExecute(Expression<Func<object, bool>> canExecuteExpression)
        {
            ObserveCanExecuteInternal(canExecuteExpression);
            return this;
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="RelayCommand"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.</param>
        /// <returns>Constructed instance of <see cref="RelayCommand"/></returns>
        public static RelayCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new RelayCommand(executeMethod);
        }

        /// <summary>
        /// Factory method to create a new instance of <see cref="RelayCommand"/> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <returns>Constructed instance of <see cref="RelayCommand"/></returns>
        public static RelayCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new RelayCommand(executeMethod, canExecuteMethod);
        }

        ///<summary>
        /// Executes the command.
        ///</summary>
        public virtual Task Execute()
        {
            return Execute(null);
        }

        /// <summary>
        /// Determines if the command can be executed.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command can execute, otherwise returns <see langword="false"/>.</returns>
        public virtual bool CanExecute()
        {
            return CanExecute(null);
        }

        protected RelayCommand(Func<Task> executeMethod)
            : this(executeMethod, () => true)
        {
        }

        protected RelayCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base((o) => executeMethod(), (o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), Resources.RelayCommandDelegatesCannotBeNull);
        }

    }
}
