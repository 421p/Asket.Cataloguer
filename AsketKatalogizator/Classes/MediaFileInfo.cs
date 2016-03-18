using System;
using System.IO;
using System.Text;
using System.Windows;

namespace AsketKatalogizator {
    internal class MediaFileInfo {
        private readonly FileInfo _fi;

        public string Name => _fi.Name;
        public string FullName => _fi.FullName;
        public bool Exists => _fi.Exists;
        public long Size => _fi.Length;
        public DateTime CreationTime => _fi.CreationTime;

        public string Md5 {
            get {
                try {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                        using (var stream = File.OpenRead(_fi.FullName))
                            return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
                catch (IOException e) {
                    MessageBox.Show($"{e.Message}\nбез понятия как это фиксить :/");
                    return string.Empty;
                }
            }
        }

        public MediaFileInfo(FileInfo fi) {
            _fi = fi;
        }

        public MediaFileInfo(string path) {
            _fi = new FileInfo(path);
        }
    }
}