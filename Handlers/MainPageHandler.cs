using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3.Model;
using S3Browser.Model;
using S3Browser.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace S3Browser.Handlers
{
    class MainPageHandler
    {
        private readonly S3ClientHandler s3ClientHandler = new S3ClientHandler();
        private object[] clearObjects;
        private ComboBox ProfileComboBox;
        private ComboBox RegionComboBox;
        private Grid SpinnerFrame;
        private ListView BucketList;
        private ListView ObjectList;
        private StackPanel NavigationPanel;
        private string BucketName;
        private string FolderKey;

        internal void Initialize(ComboBox regionComboBox, ComboBox profileComboBox)
        {
            IEnumerable<RegionEndpoint> regionEndpoints = RegionEndpoint.EnumerableAllRegions;
            foreach (RegionEndpoint regionEndpoint in regionEndpoints)
            {
                _ = regionComboBox.Items.Add(regionEndpoint);
            }

            List<CredentialProfile> profiles = this.s3ClientHandler.ListProfiles();

            if (profiles.Count() > 0)
            {
                foreach (CredentialProfile profile in profiles)
                {
                    _ = profileComboBox.Items.Add(profile);
                }
            }
        }

        internal void SetClearObjects(params object[] items)
        {
            this.clearObjects = items;
        }

        internal void SetSpinerFrame(Grid SpinnerFrame)
        {
            this.SpinnerFrame = SpinnerFrame;
        }

        internal async void GetSelectedObjectList(ListView list)
        {
            if (list.SelectedItems.Count >= 1)
            {
                await GetObjectList(this.s3ClientHandler.GetBucket(list.SelectedIndex).BucketName, "");
            }
        }

        internal void SetProfileComboBox(ComboBox ProfileComboBox)
        {
            this.ProfileComboBox = ProfileComboBox;
        }

        internal void SetRegionComboBox(ComboBox RegionComboBox)
        {
            this.RegionComboBox = RegionComboBox;
        }

        internal void SetBucketList(ListView BucketList)
        {
            this.BucketList = BucketList;
        }

        internal void SetObjectList(ListView ObjectList)
        {
            this.ObjectList = ObjectList;
        }

        internal void SetNavigationPanel(StackPanel NavigationPanel)
        {
            this.NavigationPanel = NavigationPanel;
        }

        internal void ClearList()
        {
            foreach(object obj in this.clearObjects)
            {
                if(obj is ItemCollection)
                {
                    (obj as ItemCollection).Clear();
                } else if(obj is UIElementCollection)
                {
                    (obj as UIElementCollection).Clear();
                }
            }
        }

        private void DownloadObject(S3Objects s3Objects)
        {
            var request = new GetObjectRequest
            {
                BucketName = s3Objects.BucketName
                ,
                Key = s3Objects.Key
            };

            if (this.s3ClientHandler.GetObject(request, "C:\\Download\\" + s3Objects.GetFileName()))
            {
                MessageBox.Show("Download Completed !");
            }
            else
            {
                MessageBox.Show("Download Failed !");
            }
        }

        internal async void DeleteObject(Hyperlink hyperlink)
        {
            var key = hyperlink.Tag.ToString();

            var result = MessageBox.Show("want to delete?", "Object delete", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.StartSpinnerFrame();

                this.s3ClientHandler.DeleteObject(this.BucketName, key);
                await GetObjectList(this.BucketName, this.FolderKey);

                this.EndSpinnerFrame();
            }
        }

        internal void PutObject(DragEventArgs e)
        {
            if (this.BucketName != null)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    this.StartSpinnerFrame();

                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var file = files[0];
                    System.Diagnostics.Debug.WriteLine(file);

                    var result = MessageBox.Show("want to encrypt with KMS Key?", "Selection", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        KMSEncryptUpload(file);
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        UploadFileToS3(file);
                    }

                    this.EndSpinnerFrame();
                }
            }
        }

        internal async void GetObject(ListViewItem selected)
        {
            if (selected != null)
            {

                this.StartSpinnerFrame();

                var s3Object = selected.Content as S3Objects;

                if (s3Object.Type == "Folder")
                {
                    await GetObjectList(s3Object.BucketName, s3Object.Key);
                }
                else
                {
                    DownloadObject(s3Object);
                }

                this.EndSpinnerFrame();

            }
        }

        private void KMSEncryptUpload(string filePath)
        {

            FileStream inputStream = File.OpenRead(filePath);

            string[] FilePaths = inputStream.Name.Split('\\');
            string FileName = FilePaths[FilePaths.Length - 1];

            PutObjectRequest request = null;

            if (inputStream.Length <= 4096)
            {
                if (this.s3ClientHandler.Encrypt(inputStream, out MemoryStream encStream))
                {
                    MessageBox.Show("Encryption Failed !");
                    return;
                }

                request = new PutObjectRequest
                {
                    BucketName = this.BucketName
                ,
                    Key = this.FolderKey + FileName
                ,
                    InputStream = encStream
                };

            }
            else
            {
                request = new PutObjectRequest
                {
                    BucketName = this.BucketName
                ,
                    Key = this.FolderKey + FileName
                ,
                    InputStream = inputStream
                };
            }

            S3Upload(request);

            inputStream.Close();

        }

        private void UploadFileToS3(string filePath)
        {
            var request = new PutObjectRequest
            {
                BucketName = this.BucketName
                ,
                Key = this.FolderKey
                ,
                FilePath = filePath
            };

            S3Upload(request);

        }

        private async void S3Upload(PutObjectRequest request)
        {
            if (this.s3ClientHandler.PutObject(request))
            {
                await GetObjectList(this.BucketName, "");
                MessageBox.Show("Upload Completed !");
            }
            else
            {
                MessageBox.Show("Upload Failed !");
            }
        }

        private void StartSpinnerFrame()
        {
            SpinnerFrame.Visibility = Visibility.Visible;
        }

        private void EndSpinnerFrame()
        {
            SpinnerFrame.Visibility = Visibility.Hidden;
        }

        internal void ChangeProfile()
        {
            this.s3ClientHandler.SetProfile(this.ProfileComboBox.SelectedItem as CredentialProfile);
            if (this.RegionComboBox.SelectedItem != null)
            {
                this.SearchBucketsList();
            }
            else
            {
                this.ClearList();
            }
        }

        internal void ChangeRegion()
        {
            this.s3ClientHandler.SetRegionEndpoint(this.RegionComboBox.SelectedItem as RegionEndpoint);
            if (this.ProfileComboBox.SelectedItem != null)
            {
                this.SearchBucketsList();
            }
            else
            {
                this.ClearList();
            }
        }

        private async void SearchBucketsList()
        {
            this.StartSpinnerFrame();
            this.ClearList();

            List<S3Bucket> buckets = await this.s3ClientHandler.ListBuckets();

            foreach (S3Bucket bucket in buckets)
            {
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                stackPanel.Children.Add(new Image
                {
                    Source = ResourceHelper.GetBitmapFrameForResourceImage(S3Browser.Properties.Resources.Amazon_Simple_Storage_Service_S3_Bucket_light_bg_4x)
                    ,
                    Width = 20
                    ,
                    Height = 20
                    ,
                    Margin = new Thickness(0, 0, 10, 0)
                });

                TextBlock textBlock = new TextBlock
                {
                    Text = bucket.BucketName
                };
                stackPanel.Children.Add(textBlock);

                ListViewItem item = new ListViewItem
                {
                    Content = stackPanel
                };

                _ = BucketList.Items.Add(item);
            }

            this.EndSpinnerFrame();
        }

        private Label GetHyperLinkLabel(string text, string folderKey, RoutedEventHandler target)
        {
            var run = new Run(text)
            {
                Tag = folderKey
            };

            Hyperlink hyperLink = new Hyperlink(run);
            hyperLink.Click += target;

            Label label = new Label
            {
                Content = hyperLink
            };

            return label;
        }

        private async void RefreshFolder(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            Run run = hyperlink.Inlines.FirstInline as Run;
            await GetObjectList(this.BucketName, run.Tag.ToString());
        }

        private async Task GetObjectList(string BucketName, string Prefix)
        {
            this.StartSpinnerFrame();

            this.BucketName = BucketName;
            this.FolderKey = Prefix;

            this.NavigationPanel.Children.Clear();

            this.NavigationPanel.Children.Add(this.GetHyperLinkLabel(BucketName, "", RefreshFolder));

            if (this.FolderKey != "")
            {
                string[] FolderKeys = this.FolderKey.Split('/');

                string FolderKeyOrigin = "";

                foreach (string Key in FolderKeys)
                {
                    FolderKeyOrigin += Key + "/";

                    if (Key != "")
                    {
                        this.NavigationPanel.Children.Add(new Label { Content = " > " });
                        int iChild = this.NavigationPanel.Children.Add(this.GetHyperLinkLabel(FolderKey, FolderKeyOrigin, RefreshFolder));
                    }
                }
            }

            var request = new ListObjectsV2Request
            {
                BucketName = BucketName
                ,
                Prefix = Prefix
                ,
                Delimiter = "/"
            };

            ObjectList.Items.Clear();

            ListObjectsV2Response response;

            do
            {
                response = await this.s3ClientHandler.ListObjectsV2Async(request);

                foreach (string commonPrefix in response.CommonPrefixes)
                {
                    var s3Objects = new S3Objects
                    {
                        Image = ResourceHelper.GetBitmapFrameForResourceImage(S3Browser.Properties.Resources.blue_folder)
                        ,
                        Type = "Folder"
                        ,
                        BucketName = BucketName
                        ,
                        Key = commonPrefix
                        ,
                        Name = this.FolderKey.Length > 0 ? commonPrefix.Replace(this.FolderKey, "") : commonPrefix
                    };

                    ListViewItem item = new ListViewItem
                    {
                        Content = s3Objects
                    };

                    _ = this.ObjectList.Items.Add(item);
                }

                foreach (S3Object s3Object in response.S3Objects)
                {
                    var s3Objects = new S3Objects
                    {
                        Image = ResourceHelper.GetBitmapFrameForResourceImage(S3Browser.Properties.Resources.document)
                        ,
                        Type = "Object"
                        ,
                        BucketName = s3Object.BucketName
                        ,
                        Key = s3Object.Key
                        ,
                        Name = this.FolderKey.Length > 0 ? s3Object.Key.Replace(this.FolderKey, "") : s3Object.Key
                        ,
                        LastModified = s3Object.LastModified
                        ,
                        Owner = s3Object.Owner
                        ,
                        StorageClass = s3Object.StorageClass

                    };

                    s3Objects.SetSize(s3Object.Size);

                    _ = this.ObjectList.Items.Add(new ListViewItem { Content = s3Objects });
                }

                request.ContinuationToken = response.NextContinuationToken;

                this.EndSpinnerFrame();

            } while (response.IsTruncated);

        }

    }
}
