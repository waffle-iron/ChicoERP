
using System;
using System.Windows;

#if NETFX_CORE
using Windows.UI.Xaml;
#endif

namespace Chaps.MVVM
{
    public static class ViewModelLocator
    {
        public static DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), new PropertyMetadata(false, AutoWireViewModelChanged));
        public static bool GetAutoWireViewModel(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoWireViewModelProperty);
        }
        public static void SetAutoWireViewModel(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoWireViewModelProperty, value);
        }
        private static void AutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue)
                ViewModelLocationProvider.AutoWireViewModelChanged(d, Bind);
        }



        public static void Bind(object view, object viewModel)
        {
            FrameworkElement element = view as FrameworkElement;
            if(element != null && viewModel != null)
            {
                element.DataContext = viewModel;
            }
        }
    }
}
