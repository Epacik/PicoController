using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core;

public interface IFileSystem
{
    void CreateDirectory(string path);
    bool DirectoryExists(string directory);
    bool FileExists(string path);
    string? FileReadAllText(string path);
    Task<string> FileReadAllTextAsync(string path, CancellationToken? token = null);
    void FileWriteAllText(string path, string contents);
    string[]? GetDirectories(string directory);
    Task WriteAllTextAsync(string path, string contents, CancellationToken? token = null);
}

internal class FileSystem : IFileSystem
{
    public void CreateDirectory(string path)
        => Directory.CreateDirectory(path);

    public bool DirectoryExists(string directory)
        => Directory.Exists(directory);

    public bool FileExists(string path)
        => File.Exists(path);

    public string? FileReadAllText(string path)
        => File.ReadAllText(path);

    public Task<string> FileReadAllTextAsync(string path, CancellationToken? token = null)
        => File.ReadAllTextAsync(path, cancellationToken: token ?? default);

    public void FileWriteAllText(string path, string contents)
        => File.WriteAllText(path, contents);

    public string[]? GetDirectories(string directory)
        => Directory.GetDirectories(directory);

    public Task WriteAllTextAsync(string path, string contents, CancellationToken? token = null)
        => File.WriteAllTextAsync(path, contents, cancellationToken: token ?? default);
}

