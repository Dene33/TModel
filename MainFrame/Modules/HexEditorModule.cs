#if !RELEASE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using static TModel.ColorConverters;
using TModel;
using TModel.MainFrame.Modules;
using TModel.MainFrame.Widgets;

namespace TModel.MainFrame.Modules
{
    public class HexEditorModule : ModuleBase
    {
        public override string ModuleName => "Hex Editor";

        CScrollViewer scrollViewer = new CScrollViewer();

        public StackPanel MasterPanel = new StackPanel();

        public static HexEditorModule Instance;

        public HexEditorModule() : base()
        {
            Instance = this;
            Background = HexBrush("#092041");
            scrollViewer.Content = MasterPanel;
            Content = scrollViewer;
        }
    }
}

#endif