namespace TeamCity.CSharpInteractive
{
    using System.Collections.Generic;
    using System.IO;

    internal interface IFileSystem
    {
        void DeleteDirectory(string path, bool recursive);

        bool IsPathRooted(string path);

        bool IsFileExist(string path);
        
        bool IsDirectoryExist(string path);

        IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
    }
}