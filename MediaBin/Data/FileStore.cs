﻿using LocalFile = MediaBin.Models.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace MediaBin.Data
{
    public class FileStore
    {
        private string _path;

        public IList<LocalFile> Files { get => GetFiles(); }

        public FileStore(string path)
        {
            _path = path;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public IList<LocalFile> GetFiles()
        {
            var fileList = Directory.GetFiles(_path);

            var files = new List<LocalFile>();
            foreach (var fileName in fileList)
            {
                var localFile = new LocalFile
                {
                    FileName = Path.GetFileName(fileName),
                    CreationDate = File.GetCreationTimeUtc(Path.Combine(_path, fileName)),
                };

                files.Add(localFile);
            }

            return files;
        }

        public async Task AddAsync(LocalFile file)
        {
            if (file.Content == null)
            {
                throw new ArgumentNullException($"{nameof(file.Content)} is null.");
            }

            if (Exists(file))
            {
                throw new FileExistsException($"File {file.FileName} already exists.");
            }

            await File.WriteAllBytesAsync(Path.Combine(_path, file.FileName), file.Content);
        }

        public async Task UpdateAsync(LocalFile file)
        {
            if (file.Content == null)
            {
                throw new ArgumentNullException($"{nameof(file.Content)} is null.");
            }

            if (!Exists(file))
            {
                throw new FileNotFoundException($"File {file.FileName} not found.");
            }

            await File.WriteAllBytesAsync(Path.Combine(_path, file.FileName), file.Content);
        }

        public async Task<byte[]> ReadAsync(LocalFile file)
        {
            return await File.ReadAllBytesAsync(Path.Combine(_path, file.FileName));
        }

        public void Remove(LocalFile file)
        {
            File.Delete(Path.Combine(_path, file.FileName));
        }

        public LocalFile Rename(LocalFile file, string newName)
        {
            File.Move(Path.Combine(_path, file.FileName), Path.Combine(_path, newName));

            var localFile = new LocalFile
            {
                FileName = Path.GetFileName(newName),
                CreationDate = File.GetCreationTimeUtc(Path.Combine(_path, newName)),
            };

            return localFile;

        }

        public bool Exists(LocalFile file)
        {
            return File.Exists(Path.Combine(_path, file.FileName));
        }
    }

    public class FileExistsException : Exception
    {
        public FileExistsException()
        {
        }

        public FileExistsException(string message)
            : base(message)
        {
        }

        public FileExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
