using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsketKatalogizator {
    // means Media Guy That Watching Over Files

    class MediaGTWOF {
        private readonly string _handledDir;
        private MediaWatcher _watcher;

        public MediaGTWOF(string pathToMediaDirectroy):this(pathToMediaDirectroy, null) {}

        public MediaGTWOF(string pathToMediaDirectroy, MediaWatcher watcher) {
            _handledDir = pathToMediaDirectroy;
            _watcher = watcher;
        }

        public MediaGTWOF SetRaisingEvents(bool flag) {
            _watcher.EnableRaisingEvents = flag;
            return this;
        }
        
        /// <summary>
        /// Adds MediaWatcher.
        /// </summary>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public MediaGTWOF AttachWatcher(MediaWatcher watcher) {
            _watcher = watcher;
            
            return this;
        }

        /// <summary>
        /// Returns list of all files in directory.
        /// </summary>
        public List<MediaFileInfo> Files {
            get {
                return Directory.GetFiles(_handledDir, "*.*", SearchOption.AllDirectories)
                .Select(path => new MediaFileInfo(path)).ToList();
            }
        }

        /// <summary>
        /// Returns list of files that matches tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public List<MediaFileInfo> SearchByTags(string tags) =>
            _watcher.CurrentConnection.FetchAll(
                "select distinct files.path from Tokenize('" + tags.ToLower() + @"', ' ')
                inner join tags on result = name
                inner join files2tags on tags.id = files2tags.tag_id
                inner join files on files2tags.file_id = files.id;
            ").Select(row => new MediaFileInfo((string)row.ElementAt(0))).ToList();

        /// <summary>
        /// Returns a TagCollection for file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public TagsCollection GetTagsForFile(string path) =>
            _watcher.CurrentConnection.FetchAll(@"
                            select tags.name as Tag from files
                            inner join files2tags on
                            files.id = files2tags.FILE_ID
                            inner join tags on
                            files2tags.TAG_ID = tags.id
                            where files.path = '" + path + "';")
            .Aggregate(new TagsCollection(), (collection, row) => {
                collection.Add((string) row[0]);
                return collection;
            });

    }


}
