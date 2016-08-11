using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chaps.Command
{
    /// <summary>
    /// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    public abstract class RelayCommandBase : ICommand
    {
        private bool _isActive;

        private SynchronizationContext _synchronizationContext;

        readonly HashSet<string> _porpertiesToObserve = new HashSet<string>();
        private INotifyPropertyChanged _inpc;

        protected readonly Func<object, Task> _executeMethode;
        protected Func<object, bool> _canExecuteMethode;

        /// <summary>
        /// Creates a new instance of a <see cref="RelayCommandBase"/>, specifying both the execute action and the can execute function.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to execute when <see cref="ICommand.Execute"/> is invoked.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{Object,Bool}"/> to invoked when <see cref="ICommand.CanExecute"/> is invoked.</param>
        protected RelayCommandBase(Action<object> executeMethode, Func<object, bool> canExecuteMethode)
        {
            if (executeMethode == null || canExecuteMethode == null)
                throw new ArgumentNullException(nameof(executeMethode), Resources.RelayCommandDelegatesCannotBeNull);

            _executeMethode = (arg) => { executeMethode(arg); return Task.Delay(0); };
            _canExecuteMethode = canExecuteMethode;
            _synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="RelayCommandBase"/>, specifying both the Execute action as an awaitable Task and the CanExecute function.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Func{Object,Task}"/> to execute when <see cref="ICommand.Execute"/> is invoked.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{Object,Bool}"/> to invoked when <see cref="ICommand.CanExecute"/> is invoked.</param>
        protected RelayCommandBase(Func<object, Task> executeMethode, Func<object, bool> canExecuteMethode)
        {
            if (executeMethode == null || canExecuteMethode == null)
                throw new ArgumentNullException(nameof(executeMethode), Resources.RelayCommandDelegatesCannotBeNull);

            _executeMethode = executeMethode;
            _canExecuteMethode = canExecuteMethode;
            _synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public virtual event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> so every 
        /// command invoker can requery <see cref="ICommand.CanExecute"/>.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if(handler != null)
            {
                if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                    _synchronizationContext.Post((o) => handler.Invoke(this, EventArgs.Empty), null);
                else
                    handler.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises <see cref="RelayCommandBase.CanExecuteChanged"/> so every command invoker
        /// can requery to check if the command can execute.
        /// <remarks>Note that this will trigger the execution of <see cref="RelayCommandBase.CanExecute"/> once for each invoker.</remarks>
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }


        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        
        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }

        /// <summary>
        /// Determines if the command can execute with the provided parameter by invoking the <see cref="Func{Object,Bool}"/> supplied during construction.
        /// </summary>
        /// <param name="parameter">The parameter to use when determining if this command can execute.</param>
        /// <returns>Returns <see langword="true"/> if the command can execute.  <see langword="False"/> otherwise.</returns>
        protected virtual bool CanExecute(object parameter)
        {
            return _canExecuteMethode(parameter);
        }

        /// <summary>
        /// Executes the command with the provided parameter by invoking the <see cref="Action{Object}"/> supplied during construction.
        /// </summary>
        /// <param name="parameter"></param>
        protected virtual async Task Execute(object parameter)
        {
            await _executeMethode(parameter);
        }

        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        protected internal void ObservePropertyInternal<T>(Expression<Func<T>> propertyExpression)
        {
            AddPropertyToObserver(PropertySupport.ExtractPropertyName(propertyExpression));
            HookInpc(propertyExpression.Body as MemberExpression);
        }

        /// <summary>
        /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call RelayCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute((o) => PropertyName).</param>
        /// <returns>The current instance of RelayCommand</returns>
        protected internal void ObserveCanExecuteInternal(Expression<Func<object, bool>> canExecuteExpression)
        {
            _canExecuteMethode = canExecuteExpression.Compile();
            AddPropertyToObserver(PropertySupport.ExtractPropertyNameFromLambda(canExecuteExpression));
            HookInpc(canExecuteExpression.Body as MemberExpression);
        }

        protected void AddPropertyToObserver(string property)
        {
            if (_porpertiesToObserve.Contains(property))
                throw new ArgumentException(String.Format("{0} is already being observed.", property));

            _porpertiesToObserve.Add(property);
        }

        protected void HookInpc(MemberExpression expression)
        {
            if (expression == null) return;

            if(_inpc == null)
            {
                var constantExpression = expression.Expression as ConstantExpression;
                if(constantExpression != null)
                {
                    _inpc = constantExpression.Value as INotifyPropertyChanged;
                    if (_inpc != null)
                        _inpc.PropertyChanged += Inpc_PropertyChanged;
                }
            }
        }

        void Inpc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_porpertiesToObserve.Contains(e.PropertyName))
                RaiseCanExecuteChanged();
        }

        #region IsActive
        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value><see langword="true" /> if the object is active; otherwise <see langword="false" />.</value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if(_isActive != value)
                {
                    _isActive = value;
                    OnIsActiveChanged();
                }
            }
        }

        /// <summary>
        /// Fired if the <see cref="IsActive"/> property changes.
        /// </summary>
        public virtual event EventHandler IsActiveChanged;

        /// <summary>
        /// This raises the <see cref="RelayCommandBase.IsActiveChanged"/> event.
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
            EventHandler isActiveChangedHandler = IsActiveChanged;
            if (isActiveChangedHandler != null) isActiveChangedHandler(this, EventArgs.Empty);
        }

        #endregion
    }
}
