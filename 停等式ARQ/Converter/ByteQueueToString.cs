﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace 停等式ARQ.Converter
{
    public class ByteQueueToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Queue<char> source = value as Queue<char>;
            if (source == null)
                return null;
            string goalValue = "";
            foreach (var item in source.ToArray())
            {
                goalValue += item;
            }
            return goalValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
