using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace S3Browser.Utils
{
    /// <summary>
    /// 리스트뷰 컬럼들을 리스트뷰 컨트롤에 맞게 Fill 하는 클래스
    /// </summary>
    public class ListViewColumns : DependencyObject
    {
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.RegisterAttached("Stretch", typeof(Boolean), typeof(ListViewColumns),
            new UIPropertyMetadata(true, null,
                (o, e) =>
                {
                    ListView lv = o as ListView;
                    if (lv == null)
                    {
                        throw new ArgumentException("This property may only be used on ListViews");
                    }

                    lv.Loaded += lv_Loaded;
                    lv.SizeChanged += lv_SizeChanged;

                    return e;
                }
            )
            );

        public static bool GetStretch(DependencyObject obj)
        {
            return (bool)obj.GetValue(StretchProperty);
        }

        public static void SetStretch(DependencyObject obj, bool value)
        {
            obj.SetValue(StretchProperty, value);
        }

        static void lv_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;

            // Set initial column widths
            SetColumnWidths(lv);
        }

        static void lv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            SetColumnWidths(lv);
        }

        private static void SetColumnWidths(ListView listView)
        {
            // Read unsized columns from Tag property
            List<GridViewColumn> columns = listView.Tag as List<GridViewColumn>;
            double specifiedWidth = 0;
            GridView gridView = listView.View as GridView;

            if (gridView != null)
            {
                if (columns == null)
                {
                    // Create columns list if first run
                    columns = new List<GridViewColumn>();

                    // Get all unsized columns
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (column.Width < 0)
                        {
                            specifiedWidth += column.ActualWidth;
                        }
                        else
                        {
                            columns.Add(column);
                        }
                    }
                }
                else
                {
                    // Get all unsized columns
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (columns.Contains(column) == false)
                        {
                            specifiedWidth += column.ActualWidth;
                        }
                    }
                }

                // Share remaining width equally among unsized columns
                double decSpace = 10;
                if (columns.Count > 0)
                {
                    decSpace = decSpace / columns.Count;
                }

                if (listView.ActualWidth > 0)
                {
                    foreach (GridViewColumn column in columns)
                    {
                        double newWidth = (listView.ActualWidth - specifiedWidth) / columns.Count;
                        if (newWidth >= 0)
                        {
                            column.Width = newWidth - decSpace;
                        }
                    }

                    // Store the unsized columns for later use
                    listView.Tag = columns;
                }
            }
        }
    }
}