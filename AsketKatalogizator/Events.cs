using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AsketKatalogizator {
    public partial class MainWindow : Window {

        private void ButtonForAddingTags_OnClick(object sender, RoutedEventArgs e) {

            Array.ForEach(textBoxForAddingTags.Text.Split(' '), tag => 
                _conn.ExecuteUpdate($"execute procedure bindTagToFile(lower('{tag}')," +
                                    $"'{((MediaFileInfo)listBox?.SelectedItem)?.FullName}');"));
            
            RenderOutputForFile();
            textBoxForAddingTags.Text = string.Empty;

        }

        private void textBoxForSearchByName_TextChanged(object sender, TextChangedEventArgs e)
        {
            listBox.Items.Clear();

            try
            { // search file by name
                _overwatch.Files
                    .Where(file => Regex.IsMatch(file.Name, textBoxForSearchByName.Text, RegexOptions.IgnoreCase))
                    .ToList().ForEach(file => {
                        listBox.Items.Add(file);
                    });
            }
            catch (ArgumentException)
            {
                textBoxForSearchByName.Text = new string( // prevents user from making errors in regex
                    textBoxForSearchByName.Text.ToCharArray()
                    .Take(textBoxForSearchByName.Text.Length - 1)
                    .ToArray()
                );
            }
        }

        private void ButtonForDeleteLostFile_OnClick(object sender, RoutedEventArgs e)
        {
            string path;
            if ((path = ((string)listBoxForLostFiles.SelectedItem)) == null)
            {
                MessageBox.Show("pls select some files to delete");
                return;
            }

            _conn.ExecuteUpdate($"execute procedure dropFile('{path}');");
            listBoxForLostFiles.Items.Remove(path);

        }

        private void textBoxForTagSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            listBox.Items.Clear();

            if (textBoxForTagSearch.Text == string.Empty)
            {
                _overwatch.Files.ForEach(x => listBox.Items.Add(x));
                return;
            }

            _overwatch.SearchByTags(textBoxForTagSearch.Text)?.ForEach(file =>
                listBox.Items.Add(file)
            );
        }

        private void buttonForDeletingTag_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem == null) return;
            string selected;
            if ((selected = ((string)listBoxForTags.SelectedItem)) == null) {
                MessageBox.Show("select any tag pls.");
            }

            _conn.ExecuteUpdate($"execute procedure removeTagFromFile('{selected}'," +
                                $"'{((MediaFileInfo)listBox.SelectedItem).FullName}');");

            RenderOutputForFile();
        }

        private void TextBoxForAddingTags_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ButtonForAddingTags_OnClick(null, null);
            }
        }

        /// <summary>
        /// do not invoke
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label1_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            SendMessage(new IntPtr(0xffff), 0x0010, 0, 0);
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, long msg, long wp, long lp);
    }


}
