using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chaps.MVVM
{
    public static class ViewModelLocationProvider
    {
        static Dictionary<string, Func<object>> _factories = new Dictionary<string, Func<object>>();

        static Dictionary<string, Type> _typeFactories = new Dictionary<string, Type>();

        static Func<Type, object> _defaultViewModelFactory = type => Activator.CreateInstance(type);

        static Func<Type, Type> _defaultViewTypeToViewModelTypeResolver =
            viewType =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
                return Type.GetType(viewModelName);
            };

        public static void SetDefaultViewModelFactory(Func<Type, object> viewModelFactory)
        {
            _defaultViewModelFactory = viewModelFactory;
        }

        public static void SetDefaultViewTypeToViewModelTypeResolver(Func<Type,Type> viewTypeToViewModelTypeResolver)
        {
            _defaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }


        public static void AutoWireViewModelChanged(object view, Action<object, object> setDataContextCallback)
        {
            object viewModel = GetViewModelForView(view);

            if(viewModel == null)
            {
                var viewModelType = GetViewModelTypeForView(view.GetType());

                if(viewModelType == null)
                    viewModelType = _defaultViewTypeToViewModelTypeResolver(view.GetType());

                if (viewModelType == null)
                    return;

                viewModel = _defaultViewModelFactory(viewModelType);
                
            }

            setDataContextCallback(view, viewModel);
        }

        private static Type GetViewModelTypeForView(Type view)
        {
            var viewKey = view.ToString();

            if (_typeFactories.ContainsKey(viewKey))
                return _typeFactories[viewKey];

            return null;
        }

        private static object GetViewModelForView(object view)
        {
            var viewKey = view.GetType().ToString();

            if (_factories.ContainsKey(viewKey))
                return _factories[viewKey]();

            return null;
        }

        #region Register
        public static void Register<T>(Func<object> factory)
        {
            Register(typeof(T), factory);
        }

        public static void Register(Type viewType, Func<object> factory)
        {
            Register(viewType.ToString(), factory);
        }

        public static void Register(string viewTypeName, Func<object> factory)
        {
            _factories[viewTypeName] = factory;
        }

        public static void Register<T, VM>()
        {
            Register(typeof(T), typeof(VM));
        }

        public static void Register<T>(Type viewModelType)
        {
            Register(typeof(T), viewModelType);
        }

        public static void Register(Type viewType, Type viewModelType)
        {
            Register(viewType.ToString(), viewModelType);
        }

        public static void Register(string viewTypeName, Type viewModelType)
        {
            _typeFactories[viewTypeName] = viewModelType;
        }

        #endregion

    }
}
