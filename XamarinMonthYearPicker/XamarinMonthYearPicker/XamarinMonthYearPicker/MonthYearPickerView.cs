using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinMonthYearPicker
{
    public class MonthYearPickerView : View
    {

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
          propertyName: nameof(FontSize),
          returnType: typeof(double),
          declaringType: typeof(MonthYearPickerView),
          defaultValue: (double)24,
          defaultBindingMode: BindingMode.TwoWay);

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: nameof(TextColor),
            returnType: typeof(Color),
            declaringType: typeof(MonthYearPickerView),
            defaultValue: Color.White,
            defaultBindingMode: BindingMode.TwoWay);

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }


        public static readonly BindableProperty InfiniteScrollProperty = BindableProperty.Create(
            propertyName: nameof(InfiniteScroll),
            returnType: typeof(bool),
            declaringType: typeof(MonthYearPickerView),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

        public bool InfiniteScroll
        {
            get => (bool)GetValue(InfiniteScrollProperty);
            set => SetValue(InfiniteScrollProperty, value);
        }


        public static readonly BindableProperty DateProperty = BindableProperty.Create(
            propertyName: nameof(Date),
            returnType: typeof(DateTime),
            declaringType: typeof(MonthYearPickerView),
            defaultValue: default,
            defaultBindingMode: BindingMode.TwoWay);

        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
            propertyName: nameof(MaxDate),
            returnType: typeof(DateTime?),
            declaringType: typeof(MonthYearPickerView),
            defaultValue: default,
            defaultBindingMode: BindingMode.TwoWay);

        public DateTime? MaxDate
        {
            get => (DateTime?)GetValue(MaxDateProperty);
            set => SetValue(MaxDateProperty, value);
        }

        public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
            propertyName: nameof(MinDate),
            returnType: typeof(DateTime?),
            declaringType: typeof(MonthYearPickerView),
            defaultValue: default,
            defaultBindingMode: BindingMode.TwoWay);

        public DateTime? MinDate
        {
            get => (DateTime?)GetValue(MinDateProperty);
            set => SetValue(MinDateProperty, value);
        }

    }
}
