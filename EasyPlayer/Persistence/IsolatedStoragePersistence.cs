﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Caliburn.Micro;
using EasyPlayer.Messages;

namespace EasyPlayer.Persistence
{
    public class IsolatedStoragePersistence : IPersistence, IHandle<IncreaseQuotaMessage>
    {
        private static readonly ILog log = Logger.Log<IsolatedStoragePersistence>();

        private readonly IEventAggregator eventAgg;
        private List<System.Action> delayedWrites = new List<System.Action>();
        private IsolatedStorageFile iso;
        private bool disposed;

        public IsolatedStoragePersistence(IEventAggregator eventAgg)
        {
            this.iso = IsolatedStorageFile.GetUserStoreForApplication();
            this.eventAgg = eventAgg;
            this.eventAgg.Subscribe(this);
        }

        ~IsolatedStoragePersistence()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;

            if (disposing)
            {
                if (iso != null) iso.Dispose();
                iso = null;
            }
        }

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
            WriteBinaryFileToIsolatedStorage(directory, filename, fullPath, contents);
        }
        private void WriteBinaryFileToIsolatedStorage(string directory, string filename, string fullPath, Stream contents)
        {
            if (iso.AvailableFreeSpace < contents.Length)
            {
                log.Info("Not enough space to write contents to isolated storage - have to prompt user to increase quota. Current quota: {0}; Free space: {1}; Attempting to write: {2}", iso.Quota, iso.AvailableFreeSpace, contents.Length);
                eventAgg.Publish(new OutOfQuotaMessage(iso.Quota, iso.AvailableFreeSpace, contents.Length));

                delayedWrites.Add(() => WriteBinaryFileToIsolatedStorage(directory, filename, fullPath, contents));

                return;
            }

            log.Info("Writing to isolated storage {0}", fullPath);

            if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                iso.CreateDirectory(directory);

            using (var file = iso.OpenFile(fullPath, FileMode.Create))
                contents.CopyTo(file);
        }

        public IEnumerable<string> Filenames()
        {
            return Filenames("");
        }
        public IEnumerable<string> Filenames(string inDirectory)
        {
            var filePattern = string.IsNullOrWhiteSpace(inDirectory) ? "*.*" : Path.Combine(inDirectory, "*.*");

            if (!string.IsNullOrWhiteSpace(inDirectory) && !iso.DirectoryExists(inDirectory))
                iso.CreateDirectory(inDirectory);

            return iso.GetFileNames(filePattern);
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

            if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                iso.CreateDirectory(directory);

            if (!iso.FileExists(fullPath)) return new MemoryStream();

            log.Info("Reading from isolated storage {0}", fullPath);

            return iso.OpenFile(fullPath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read);
        }

        public void DeleteFile(string filename)
        {
            DeleteFile("", filename);
        }
        public void DeleteFile(string directory, string filename)
        {
            var fullPath = string.IsNullOrWhiteSpace(directory) ? filename : Path.Combine(directory, filename);

            if (!string.IsNullOrWhiteSpace(directory) && !iso.DirectoryExists(directory))
                iso.CreateDirectory(directory);

            if (!iso.FileExists(fullPath)) return;
                
            log.Info("Deleting file from isolated storage {0}", fullPath);

            try
            {
                iso.DeleteFile(fullPath);
            }
            catch (IsolatedStorageException ex)
            {
                log.Warn("Error deleting file from Isolated Storage: {0}", ex);
            }
        }

        public void Handle(IncreaseQuotaMessage message)
        {
            var newQuotaSize = iso.Quota + Math.Max(message.IncreaseBy, 1024);
            log.Info("Increasing quota to {0}", newQuotaSize);
                
            var increased = iso.IncreaseQuotaTo(newQuotaSize);
            log.Info("Increased quota? {0}", increased);

            var writes = new List<System.Action>(delayedWrites);
            delayedWrites.Clear();
                
            writes.ForEach(a => a());

            log.Info("Replayed writes that were pending the quota request.");
        }
    }
}
