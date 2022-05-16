#if !RELEASE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TModel.MainFrame.Modules
{
    // If the selected item in DirectoryModule is a USkeletalMesh
    // or UStaticMesh, it will appear here.
    public class ModelViewerModule : ModuleBase
    {
        public override string ModuleName => "Model Viewer";

        public ModelViewerModule()
        {
            Content = new ReadonlyText("ModelViewer", 60) { Background = Brushes.Black };
        }
    }
}

#endif