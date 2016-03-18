using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AsketKatalogizator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private readonly PhoenixDBAL _conn;
        private readonly MediaGTWOF _overwatch;
        private readonly MediaWatcher _watcher;
        private readonly string _mediaDir;
        private List<MediaFileInfo> _filesFromDir;
        private List<string> _lostFiles; 

        public MainWindow() {
            this._mediaDir = ConfigurationManager.AppSettings["MediaDir"];
            this.Closed += (s, e) => Application.Current.Shutdown();

            _conn = new PhoenixDBAL(ConfigurationManager.ConnectionStrings["PhoenixDBAL"].ConnectionString);

            _watcher = new MediaWatcher(_conn) {Path = _mediaDir, EnableRaisingEvents = true};

            _overwatch = new MediaGTWOF(_mediaDir)
                .AttachWatcher(_watcher);
                //.SetRaisingEvents(true);

            InitializeComponent();

            Entry();

        }

        private void Entry() {

            listBox.DisplayMemberPath = "Name";

            var filesFromDatabase = _conn.FetchAll("select path, hash from files;");
            _filesFromDir = _overwatch.Files;

            filesFromDatabase.ForEach(row => {
                if (_filesFromDir.Count(x => x.FullName == (string) row[0]) == 0) {
                    listBoxForLostFiles.Items.Add((string) row[0]);
                }
                else {
                    var file = _filesFromDir.Where(x => x.FullName == (string) row[0]).ElementAt(0);
                    listBox.Items.Add(file);
                    _filesFromDir.Remove(file);
                }
            });

            _filesFromDir.ForEach(file => {
                var md5 = file.Md5;
                if (            // if row with same hash already exists in database
                    (int) _conn.FetchAll($"select count(*) from files where hash = '{md5}';")[0][0] == 0
                    ) {
                    _conn.ExecuteUpdate($"execute procedure addFile('{file.Name}','{file.FullName}','{file.Md5}');");
                    _conn.ExecuteUpdate($"execute procedure bindTagToFile('new', '{file.FullName}');");
                }
                else {
                    _conn.ExecuteUpdate($"update files set filename = '{file.Name}'," +
                                        $"path = '{file.FullName}' where hash = '{md5}';");
                }
                listBox.Items.Add(file);
            });

            listBox.SelectionChanged += (sender, e) => {
                RenderOutputForFile();
            };

            _watcher.RenderRequested += (s, e) => {
                this.Dispatcher.Invoke(() => {
                    listBox.Items.Clear();
                    _overwatch.Files.ForEach(file => listBox.Items.Add(file));
                });
            };

        }
    }
}
