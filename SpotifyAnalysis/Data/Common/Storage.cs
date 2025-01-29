using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.Common {
    public class Storage<T>(string key, ProtectedLocalStorage protectedLocalStorage) {
        private readonly string key = key;
        private readonly ProtectedLocalStorage storage = protectedLocalStorage;

        public async Task<T> Get(T backupValue = default) => await storage.GetSafeAsync(key, backupValue);
        public async Task Set(T value) => await storage.SetAsync(key, value);
    }

    public static class StorageExtensions {
        public static async Task<T> GetSafeAsync<T>(this ProtectedLocalStorage storage, string key, T backupValue = default) {
            try {
                var result = await storage.GetAsync<T>(key);
                return result.Success ? result.Value : backupValue;
            }
            catch (CryptographicException) {
                try {
                    await storage.SetAsync(key, backupValue);
                }
                catch (Exception e) {
                    // TODO log
                }
                return backupValue;
            }
        }
    }
}
