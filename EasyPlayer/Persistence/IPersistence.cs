using System;
using System.Collections.Generic;
using System.IO;

namespace EasyPlayer.Persistence
{
    public interface IPersistence : IDisposable
    {
        void WriteTextFile(string filename, string contents);
        void WriteTextFile(string directory, string filename, string contents);
        
        void WriteBinaryFile(string filename, Stream contents);
        void WriteBinaryFile(string directory, string filename, Stream contents);

        IEnumerable<string> Filenames();
        IEnumerable<string> Filenames(string inDirectory);

        string ReadTextFile(string filename);
        string ReadTextFile(string directory, string filename);

        Stream ReadBinaryFile(string filename);
        Stream ReadBinaryFile(string directory, string filename);

        void DeleteFile(string filename);
        void DeleteFile(string directory, string filename);
    }
}
