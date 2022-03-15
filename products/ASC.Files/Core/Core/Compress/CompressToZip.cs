﻿namespace ASC.Web.Files.Core.Compress;

/// <summary>
/// Archives the data stream into the format .zip
/// </summary>
[Scope]
public class CompressToZip : ICompress
{
    private ZipOutputStream _zipStream;
    private ZipEntry _zipEntry;
    private readonly TempStream _tempStream;

    public CompressToZip(TempStream tempStream)
    {
        _tempStream = tempStream;
    }

    /// <summary> </summary>
    /// <param name="stream">Accepts a new stream, it will contain an archive upon completion of work</param>
    public void SetStream(Stream stream)
    {
        _zipStream = new ZipOutputStream(stream) { UseZip64 = UseZip64.Dynamic };
        _zipStream.IsStreamOwner = false;
    }

    /// <summary>
    /// The record name is created (the name of a separate file in the archive)
    /// </summary>
    /// <param name="title">File name with extension, this name will have the file in the archive</param>
    public void CreateEntry(string title)
    {
        _zipEntry = new ZipEntry(title) { IsUnicodeText = true };
    }

    /// <summary>
    /// Transfer the file itself to the archive
    /// </summary>
    /// <param name="readStream">File data</param>
    public void PutStream(Stream readStream)
    {
        using (var buffered = _tempStream.GetBuffered(readStream))
        {
            _zipEntry.Size = buffered.Length;
            _zipStream.PutNextEntry(_zipEntry);
            buffered.CopyTo(_zipStream);
        }
    }

    /// <summary>
    /// Put an entry on the output stream.
    /// </summary>
    public void PutNextEntry()
    {
        _zipStream.PutNextEntry(_zipEntry);
    }

    /// <summary>
    /// Closes the current entry.
    /// </summary>
    public void CloseEntry()
    {
        _zipStream.CloseEntry();
    }

    /// <summary>
    /// Resource title (does not affect the work of the class)
    /// </summary>
    /// <returns></returns>
    public string Title => FilesUCResource.FilesWillBeCompressedZip;

    /// <summary>
    /// Extension the archive (does not affect the work of the class)
    /// </summary>
    /// <returns></returns>
    public string ArchiveExtension => CompressToArchive.ZipExt;

    public void Dispose()
    {
        _zipStream?.Dispose();
    }
}
