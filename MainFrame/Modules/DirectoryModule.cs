using CUE4Parse.FileProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TModel.ColorConverters;
using static CUE4Parse.Utils.StringUtils;
using System.IO;
using System.Threading;
using TModel.MainFrame.Widgets;
using CUE4Parse.UE4.Assets.Exports;
using System.Windows.Input;
using TModel.MainFrame.Modules;
using CUE4Parse.FN.Exports.FortniteGame;
using CUE4Parse.UE4.Assets.Exports.Texture;
using System.Windows.Media.Imaging;
using TModel.Export;
using CUE4Parse.UE4.Objects.UObject;

#if !RELEASE
namespace TModel.Modules
{
    /** Navigation of the game files.
        ObjectViewerModule shows the selected asset.
        If there are multiple DirectoryModules then ObjectViewerModule shows
        the most recently clicked one.
        TODO: add tile/grid view for asset panel and add custom previews on those tiles
    **/
    public class DirectoryModule : ModuleBase
    {
        public static Action<IEnumerable<UObject>?> SelectedItemChanged;

        public static Action<string> GoToPath;

        public static Dictionary<string, string> QuickPaths { get; } = new Dictionary<string, string>()
        {
            ["Characters"] = "FortniteGame/Content/Athena/Items/Cosmetics/Characters",
            ["Backpacks"] = "FortniteGame/Content/Athena/Items/Cosmetics/Backpacks",
            ["Gliders"] = "FortniteGame/Content/Athena/Items/Cosmetics/Gliders",
            ["Pickaxes"] = "FortniteGame/Content/Athena/Items/Cosmetics/PickAxes",
        };

        public override string ModuleName => "Directory";

        public DirectoryModule() : base()
        {
            GoToPath += (path) =>
            {
                CurrentPath = path.SubstringBeforeLast('/');
                LoadPath(path.SubstringBeforeLast('/'));
                foreach (var asset in AssetsPanel.Children)
                {
                    AssetItem Item = (AssetItem)asset;
                    if (Item.FullPath == path)
                    {
                        Item.Select();
                        break;
                    }
                }
            };

            FileManagerModule.FilesLoaded += RefreshStuff;

            void RefreshStuff() => LoadPath(CurrentPath);

            Grid Root = new Grid() { Background = HexBrush("#12161b") };
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            Root.RowDefinitions.Add(new RowDefinition());
            Root.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });


            Grid ItemsGrid = new Grid();
            Grid.SetRow(ItemsGrid, 1);
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });
            ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            AssetScrollViewer.Content = AssetsPanel;
            FolderScrollViewer.Content = FoldersPanel;

            Grid.SetColumn(AssetScrollViewer, 1);
            Grid.SetColumn(FolderScrollViewer, 0);

            ItemsGrid.Children.Add(AssetScrollViewer);
            ItemsGrid.Children.Add(FolderScrollViewer);

            StackPanel ButtonPanel = new StackPanel();
            Grid.SetRow(ButtonPanel, 2);
            ButtonPanel.Orientation = Orientation.Horizontal;

#if false
            for (int i = 0; i < 40; i++)
            {
                AssetsPanel.Children.Add(new DirectoryItem("TestItemDir/TestItem.debug", this, true));
            }
#endif

            // Reloads the current path.
            CButton RefreshButton = new CButton("Refresh");
            RefreshButton.Padding = new Thickness(4);
            RefreshButton.Click += () => LoadPath(CurrentPath);

            // Goes back a path
            CButton BackButton = new CButton("Back");
            BackButton.Padding = new Thickness(4);
            BackButton.Click += () =>
            {
                if (CurrentPath.Contains('/'))
                {
                    string NEwString = CurrentPath.SubstringBeforeWithLast('/');
                    LoadPath(CurrentPath = NEwString.Substring(0, NEwString.Length - 1));
                }
            };

            ComboBox QuickPathsBox = new ComboBox();
            foreach (var path in QuickPaths)
            {
                CTextBlock ItemText = new CTextBlock(path.Key) { Foreground = Brushes.Black };
                ItemText.Tag = path.Key;
                QuickPathsBox.Items.Add(ItemText);
            }

            QuickPathsBox.SelectionChanged += (sender, args) =>
            {
                string FoundPath = QuickPaths[(string)((CTextBlock)QuickPathsBox.SelectedItem).Tag];
                CurrentPath = FoundPath;
                LoadPath(FoundPath);
            };


            ButtonPanel.Children.Add(RefreshButton);
            ButtonPanel.Children.Add(BackButton);
            ButtonPanel.Children.Add(QuickPathsBox);



            Root.Children.Add(PathText);
            Root.Children.Add(ItemsGrid);
            Root.Children.Add(ButtonPanel);

            Content = Root;

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    App.ShowModule<SearchModule>();
                }
            };
        }

        // The path currently being shown.
        string CurrentPath = "FortniteGame/Content";

        // Left side (Folder Panel). usaully smaller than asset panel
        CScrollViewer FolderScrollViewer = new CScrollViewer();
        StackPanel FoldersPanel = new StackPanel();

        // Right side (Asset Panel).
        CScrollViewer AssetScrollViewer = new CScrollViewer();
        WrapPanel AssetsPanel = new WrapPanel();


        // Very top bar. Shows CurrentPath.
        CTextBlock PathText = new CTextBlock() 
        { 
            VerticalAlignment = VerticalAlignment.Center 
        };

        void LoadPath(string path)
        {
            List<FolderItem> Folders = new List<FolderItem>();
            List<AssetItem> Assets = new List<AssetItem>();

            IEnumerable<string> Names = Enumerable.Empty<string>();

            Task.Run(() =>
            {
                Names = App.FileProvider.GetFilesInPath(path);
            }).GetAwaiter().OnCompleted(() => 
            {
                foreach (var item in Names)
                    if (item.Contains('.'))
                        Assets.Add(new AssetItem(CurrentPath + '/' + item, this));
                    else
                        Folders.Add(new FolderItem(CurrentPath + '/' + item, this));

                // ERROR: failed to sort items in array
                // Folders.Sort();
                // Assets.Sort();

                PathText.Text = CurrentPath;

                AssetsPanel.Children.Clear();
                FoldersPanel.Children.Clear();
                AssetScrollViewer.ScrollToVerticalOffset(0);
                FolderScrollViewer.ScrollToVerticalOffset(0);

                foreach (var folder in Folders)
                    FoldersPanel.Children.Add(folder);
                foreach (var asset in Assets)
                    AssetsPanel.Children.Add(asset);
            });
        }

        static AssetItem? SelectedAsset = null;

        public class AssetItem : ContentControl
        {
            DirectoryModule Owner;

            bool IsSelected => SelectedAsset == this;

            public string FullPath;

            StackPanel Root = new StackPanel();

            Border border = new Border()
            {
                Background = Theme.BackNormal,
                BorderBrush = Theme.BorderNormal,
                BorderThickness = new Thickness(1.8),
                Margin = new Thickness(2),
                Height = 80,
            };

            public AssetItem(string path, DirectoryModule owner)
            {
                Width = 80;

                Margin = new Thickness(5);

                Owner = owner;
                FullPath = path;
                Root.Children.Add(border);

                CTextBlock NameText = new CTextBlock(Path.GetFileName(path.SubstringBeforeLast('.')), 10)
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };

                Root.Children.Add(NameText);

                MouseEnter += (sender, args) =>
                {
                    Root.Background = Theme.BackHover;
                };

                MouseLeave += (sender, args) =>
                {
                    Root.Background = Brushes.Transparent;
                };

                MouseLeftButtonDown += (sender, args) =>
                {
                    App.ShowModule<ObjectViewerModule>();
                    IEnumerable<UObject>? Exports = null;
                    Exports = App.FileProvider.LoadObjectExports(FullPath);

                    SelectedItemChanged(Exports);
                };

                Content = Root;
#if false
                Task.Run(() =>
                {
                    BitmapImage PreviewImage = new BitmapImage();

                    UObject BaseObject = App.FileProvider.LoadPackage(FullPath).ExportsLazy[0].Value;
                    App.Refresh(() =>
                    {
                        border.Child = new Image() { Source = BaseObject.GetPreviewIcon() };
                        ToolTip = new CTooltip(BaseObject.ExportType);
                    });
                });
#endif
            }

            public void Select()
            {
                // Deselects the current selected asset
                if (SelectedAsset is AssetItem item)
                    item.Deselect();
                // Selects this asset
                border.Background = Theme.BackSelected;
                SelectedAsset = this;

                // Calls a Action to let the ObjectViewer know to update its contents.
                IEnumerable<UObject>? Exports = null;
                Task.Run(() => Exports = App.FileProvider.LoadObjectExports(FullPath))
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    if (SelectedItemChanged != null)
                        SelectedItemChanged(Exports);
                });
            }

            void Deselect()
            {
                border.Background = Theme.BackNormal;
            }
        }

        public class FolderItem : ContentControl
        {
            string FullPath;
            DirectoryModule Owner;
            Border border = new Border()
            {
                Background = Theme.BackNormal,
                BorderBrush = Theme.BorderNormal,
                BorderThickness = new Thickness(1.8),
                Padding = new Thickness(5)
            };

            public FolderItem(string path, DirectoryModule owner)
            {
                FullPath = path;
                Owner = owner;
                border.Child = new CTextBlock(Path.GetFileName(path));
                Content = border;

                MouseEnter += (sender, args) =>
                {
                    border.Background = Theme.BackHover;
                };

                MouseLeave += (sender, args) =>
                {
                    border.Background = Theme.BackNormal;
                };

                MouseLeftButtonDown += (s, e) => Open();
            }

            public void Open()
            {
                Owner.CurrentPath = FullPath;
                Owner.LoadPath(FullPath);
            }
        }
    }
}
#endif