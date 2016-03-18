using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AsketKatalogizator
{
    public partial class MainWindow : Window
    {
        private void RenderOutputForFile() {
            MediaFileInfo selected;
            if ((selected = ((MediaFileInfo)listBox.SelectedItem)) == null) return;

            outputBlock.Text = $"Name: {selected.Name}\n" +
                               $"Path: {selected.FullName}\n" +
                               $"Size: {selected.Size / 1024} kbytes.\n" +
                               $"Created: {selected.CreationTime}";

            listBoxForTags.Items.Clear();
            _overwatch.GetTagsForFile(((MediaFileInfo)listBox.SelectedItem).FullName)
                .ForEach(file => listBoxForTags.Items.Add(file));
            
        }

    }
}
