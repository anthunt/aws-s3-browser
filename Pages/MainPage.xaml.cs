using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using S3Browser.Handlers;

namespace S3Browser.Pages
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly MainPageHandler mainWindowHandler = new MainPageHandler();

        public MainPage()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.mainWindowHandler.Initialize(regionComboBox, profileComboBox);
            this.mainWindowHandler.SetClearObjects(bucketList.Items, objectList.Items, navigationPanel.Children);
            this.mainWindowHandler.SetSpinerFrame(SpinnerFrame);
            this.mainWindowHandler.SetProfileComboBox(profileComboBox);
            this.mainWindowHandler.SetRegionComboBox(regionComboBox);
            this.mainWindowHandler.SetBucketList(bucketList);
            this.mainWindowHandler.SetObjectList(objectList);
            this.mainWindowHandler.SetNavigationPanel(navigationPanel);
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.mainWindowHandler.ChangeProfile();
        }

        private void RegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.mainWindowHandler.ChangeRegion();
        }
                
        private void BucketList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            this.mainWindowHandler.GetSelectedObjectList(list);
        }

        private void ObjectList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            var selected = listView.SelectedItem as ListViewItem;
            this.mainWindowHandler.GetObject(selected);
        }

        private void ObjectList_PreviewDrop(object sender, DragEventArgs e)
        {
            this.mainWindowHandler.PutObject(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            this.mainWindowHandler.DeleteObject(hyperlink);
        }

        private void ObjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            var selected = listView.SelectedItem as ListViewItem;
            ObjectInfoPanel.Visibility = Visibility.Visible;
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            ObjectInfoPanel.Visibility = Visibility.Hidden;
        }
    }
}
