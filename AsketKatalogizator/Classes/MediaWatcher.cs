using System;
using System.IO;
using System.Windows;

namespace AsketKatalogizator {
    internal class MediaWatcher : FileSystemWatcher {
        private readonly PhoenixDBAL _conn;

        public event EventHandler RenderRequested;

        protected virtual void OnRenderRequested(EventArgs e) {
            var handler = RenderRequested;
            handler?.Invoke(this, e);
        }

        public MediaWatcher(PhoenixDBAL connection):base() {
            _conn = connection;
            this.Renamed += OnFileRenamed;
            this.Created += OnFileCreated;
            this.Deleted += OnFileDeleted;
            this.Changed += OnFileChanged;

            IncludeSubdirectories = true;
            this.Filter = "*.*";
            this.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size |
                                NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.CreationTime | 
                                NotifyFilters.Attributes;
        }

        public PhoenixDBAL CurrentConnection => _conn;

        private void OnFileChanged(object sender, FileSystemEventArgs e) {
            try {
                EnableRaisingEvents = false;
                
                var changedFile = new MediaFileInfo(e.FullPath);

                _conn.ExecuteUpdate(
                    $"update files set hash = '{changedFile.Md5}' where path = '{changedFile.FullName}';"
                );

                OnRenderRequested(EventArgs.Empty);
            } finally {
                EnableRaisingEvents = true;
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e) {
            try {
                EnableRaisingEvents = false;
                var createdFile = new MediaFileInfo(e.FullPath);

                _conn.ExecuteUpdate("execute procedure addFile(" +
                                    $"'{createdFile.Name}'," +
                                    $"'{createdFile.FullName}'," +
                                    $"'{createdFile.Md5}');");

                _conn.ExecuteUpdate($"execute procedure bindTagToFile('new', '{createdFile.FullName}')");

                OnRenderRequested(EventArgs.Empty);
            } finally {
                EnableRaisingEvents = true;
            }
            
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e) {
            try {
                EnableRaisingEvents = false;
                _conn.ExecuteUpdate($"execute procedure dropFile('{e.FullPath}');");
                OnRenderRequested(EventArgs.Empty);
            } finally {
                EnableRaisingEvents = true;
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e) {
            try {
                EnableRaisingEvents = false;
                _conn.ExecuteUpdate($"update files set filename = '{e.FullPath}' where filename = '{e.OldFullPath}'");
                OnRenderRequested(EventArgs.Empty);
            } finally {
                EnableRaisingEvents = true;
            }
            
        }
    }
}