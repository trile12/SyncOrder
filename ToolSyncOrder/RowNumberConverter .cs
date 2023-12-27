using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToolSyncOrder
{
    public class RowNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is int index && values[1] is LogInfo logInfo)
            {
                return index + 1;
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ListViewItemExtensions
    {
        public static int GetIndex(this ListViewItem item)
        {
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            return listView?.ItemContainerGenerator.IndexFromContainer(item) ?? -1;
        }
    }
}
