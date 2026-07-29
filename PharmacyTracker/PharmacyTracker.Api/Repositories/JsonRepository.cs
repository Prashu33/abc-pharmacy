using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PharmacyTracker.Api.Repositories
{
    public class JsonRepository<T> : IJsonRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public JsonRepository(string filePath)
        {
            _filePath = filePath;
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<T>();
                }

                using var stream = File.OpenRead(_filePath);
                var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
                return items ?? new List<T>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T?> GetByIdAsync(Func<T, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.FirstOrDefault(predicate);
        }

        public async Task AddAsync(T entity)
        {
            await _semaphore.WaitAsync();
            try
            {
                List<T> items = new List<T>();
                if (File.Exists(_filePath))
                {
                    using (var readStream = File.OpenRead(_filePath))
                    {
                        items = (await JsonSerializer.DeserializeAsync<List<T>>(readStream, _jsonOptions)) ?? new List<T>();
                    }
                }

                items.Add(entity);
                await WriteAtomicAsync(items);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateAsync(Func<T, bool> predicate, Action<T> updateAction)
        {
            await _semaphore.WaitAsync();
            try
            {
                List<T> items = new List<T>();
                if (File.Exists(_filePath))
                {
                    using (var readStream = File.OpenRead(_filePath))
                    {
                        items = (await JsonSerializer.DeserializeAsync<List<T>>(readStream, _jsonOptions)) ?? new List<T>();
                    }
                }

                var item = items.FirstOrDefault(predicate);
                if (item != null)
                {
                    updateAction(item);
                    await WriteAtomicAsync(items);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveAllAsync(List<T> items)
        {
            await _semaphore.WaitAsync();
            try
            {
                await WriteAtomicAsync(items);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task WriteAtomicAsync(List<T> items)
        {
            var tempFilePath = Path.Combine(
                Path.GetDirectoryName(_filePath) ?? ".",
                $"{Path.GetFileNameWithoutExtension(_filePath)}_{Guid.NewGuid():N}.tmp"
            );

            using (var tempStream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(tempStream, items, _jsonOptions);
            }

            File.Move(tempFilePath, _filePath, overwrite: true);
        }
    }
}
