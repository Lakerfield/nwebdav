﻿using System;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;

using NWebDav.Server.Locking;

namespace NWebDav.Server.Stores
{
    public class DiskStore : IStore
    {
        public DiskStore(string directory, ILockingManager lockingManager = null)
        {
            BaseDirectory = directory;
            LockingManager = lockingManager ?? new InMemoryLockingManager();
        }

        public string BaseDirectory { get; }
        public ILockingManager LockingManager { get; }

        public Task<IStoreItem> GetItemAsync(Uri uri, IPrincipal principal)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IStoreItem>(new DiskStoreCollection(LockingManager, new DirectoryInfo(path)));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IStoreItem>(new DiskStoreItem(LockingManager, new FileInfo(path)));

            // The item doesn't exist
            return Task.FromResult<IStoreItem>(null);
        }

        public Task<IStoreCollection> GetCollectionAsync(Uri uri, IPrincipal principal)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);
            if (!Directory.Exists(path))
                return Task.FromResult<IStoreCollection>(null);

            // Return the item
            return Task.FromResult<IStoreCollection>(new DiskStoreCollection(LockingManager, new DirectoryInfo(path)));
        }

        private string GetPathFromUri(Uri uri)
        {
            // Determine the path
            var requestedPath = uri.LocalPath.Substring(1).Replace('/', Path.DirectorySeparatorChar);

            // Determine the full path
            var fullPath = Path.GetFullPath(Path.Combine(BaseDirectory, requestedPath));

            // Make sure we're still inside the specified directory
            if (fullPath != BaseDirectory && !fullPath.StartsWith(BaseDirectory + Path.DirectorySeparatorChar))
                throw new SecurityException($"Uri '{uri}' is outside the '{BaseDirectory}' directory.");

            // Return the combined path
            return fullPath;
        }
    }
}
