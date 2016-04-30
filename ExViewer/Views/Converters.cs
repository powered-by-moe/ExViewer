﻿using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ExViewer.Views
{
    public abstract class ValueConverterChain : IValueConverter
    {
        public IValueConverter InnerConverter
        {
            get; set;
        }

        public abstract object ConvertImplementation(object value, Type targetType, object parameter, string language);
        public abstract object ConvertBackImplementation(object value, Type targetType, object parameter, string language);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var convertedByThis = ConvertImplementation(value, targetType, parameter, language);
            if(InnerConverter == null)
                return convertedByThis;
            else
                return InnerConverter.Convert(convertedByThis, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object convertedByInner;
            if(InnerConverter != null)
                convertedByInner = InnerConverter.ConvertBack(value, targetType, parameter, language);
            else
                convertedByInner = value;
            return ConvertBackImplementation(convertedByInner, targetType, parameter, language);
        }
    }

    public class InvertConverter : IValueConverter
    {
        public IValueConverter Raw
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Raw.ConvertBack(value, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Raw.Convert(value, targetType, parameter, language);
        }
    }

    public class LoadStateToVisualStateConverter : ValueConverterChain
    {
        private static Brush accent;

        public static Brush AccentBrush
        {
            get
            {
                return System.Threading.LazyInitializer.EnsureInitialized(ref accent, () => (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"]);
            }
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var state = (ExClient.ImageLoadingState)value;
            if(targetType == typeof(Visibility))
            {
                if(state == ExClient.ImageLoadingState.Loaded)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            if(targetType == typeof(Brush))
            {
                if(state == ExClient.ImageLoadingState.Failed)
                    return new SolidColorBrush(Windows.UI.Colors.Red);
                else
                    return AccentBrush;
            }
            if(targetType == typeof(bool))
            {
                if(state == ExClient.ImageLoadingState.Waiting || state == ExClient.ImageLoadingState.Preparing)
                    return true;
                else
                    return false;
            }
            throw new NotImplementedException();
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoryToImageConverter : ValueConverterChain
    {
        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var cata = (Category)value;
            var uri = new Uri($"ms-appx:///CategoryImage/{cata.ToString()}.png");
            return new BitmapImage(uri);
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StringConverter : ValueConverterChain
    {
        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return string.Format(parameter.ToString(), value.ToString());
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : ValueConverterChain
    {
        public bool BoolearForVisible
        {
            get;
            set;
        } = true;

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v == BoolearForVisible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return ((Visibility)value == Visibility.Visible) ? BoolearForVisible : !BoolearForVisible;
        }
    }

    public class ObjectToBooleanConverter : ValueConverterChain
    {
        public object ValueForTrue
        {
            get;
            set;
        }

        public object ValueForFalse
        {
            get;
            set;
        }

        /// <summary>
        /// Returns when <c>value != ValueForTrue && value != ValueForFalse</c>.
        /// </summary>
        public bool Others
        {
            get;
            set;
        }

        /// <summary>
        /// Returns when <c>value == ValueForTrue && value == ValueForFalse</c>.
        /// </summary>
        public bool Default
        {
            get;
            set;
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            if(value == ValueForTrue && value == ValueForFalse)
                return Default;
            if(value == ValueForTrue)
                return true;
            if(value == ValueForFalse)
                return false;
            return Others;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v)
                return ValueForTrue;
            else
                return ValueForFalse;
        }
    }

    public class LengthConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = value as double?;
            if(v == null)
            {
                if(value is GridLength)
                {
                    var v2 = (GridLength)value;
                    if(v2.IsAbsolute)
                        v = v2.Value;
                    else
                        v = double.NaN;
                }
            }

            if(v == null)
                throw new InvalidOperationException();
            var d = v.Value;
            if(targetType == typeof(double))
                return d;
            if(targetType == typeof(GridLength))
                return new GridLength(d);
            throw new InvalidOperationException();
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return ConvertBackImplementation(value, targetType, parameter, language);
        }
    }
}