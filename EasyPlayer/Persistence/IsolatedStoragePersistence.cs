using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace EasyPlayer.Persistence
{
    public class IsolatedStoragePersistence : IPersistence
    {
        public void WriteTextFile(string filename, string contents)
        {
            WriteTextFile("", filename, contents);
        }
        public void WriteTextFile(string directory, string filename, string contents)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.Unicode))
            {
                writer.Write(contents);
                writer.Flush();

                stream.Position = 0;
                WriteBinaryFile(directory, filename, stream);
            }
        }

        public void WriteBinaryFile(string filename, Stream contents)
        {
            WriteBinaryFile("", filename, contents);
        }
        public void WriteBinaryFile(string directory, string filename, Stream contents)
        {
            var fullPath = string.IsNullOrWhiteSpace(directory) ? filename : Path.Combine(directory, filename);
            
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                    iso.CreateDirectory(directory);

                using (var file = iso.OpenFile(fullPath, FileMode.Create))
                    contents.CopyTo(file);
            }
        }

        public IEnumerable<string> Filenames()
        {
            return Filenames("");
        }
        public IEnumerable<string> Filenames(string inDirectory)
        {
            var filePattern = string.IsNullOrWhiteSpace(inDirectory) ? "*.*" : Path.Combine(inDirectory, "*.*");

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!string.IsNullOrWhiteSpace(inDirectory) && !iso.DirectoryExists(inDirectory))
                    iso.CreateDirectory(inDirectory);

                return iso.GetFileNames(filePattern);
            }
        }

        public string ReadTextFile(string filename)
        {
            return ReadTextFile("", filename);
        }
        public string ReadTextFile(string directory, string filename)
        {
            using (var reader = new StreamReader(ReadBinaryFile(directory, filename), Encoding.Unicode))
                return reader.ReadToEnd();
        }

        public Stream ReadBinaryFile(string filename)
        {
            return ReadBinaryFile("", filename);
        }
        public Stream ReadBinaryFile(string directory, string filename)
        {
            var fullPath = string.IsNullOrWhiteSpace(directory) ? filename : Path.Combine(directory, filename);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                    iso.CreateDirectory(directory);

                if (!iso.FileExists(fullPath)) return new MemoryStream();

                return iso.OpenFile(fullPath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read);
            }
        }

        public void DeleteFile(string filename)
        {
            DeleteFile("", filename);
        }
        public void DeleteFile(string directory, string filename)
        {
            var fullPath = string.IsNullOrWhiteSpace(directory) ? filename : Path.Combine(directory, filename);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                    iso.CreateDirectory(directory);

                if (!iso.FileExists(fullPath)) return;

                iso.DeleteFile(fullPath);
            }
        }
    }
}
