using System;
using System.IO;

namespace SaveSystem.Storage
{
    /// <summary>
    /// Хранит данные локально в виде бинарных файлов.
    ///
    /// Путь к папке задаётся снаружи — в Unity передавайте
    /// Application.persistentDataPath (для сборки) или
    /// Application.dataPath   (для работы в редакторе).
    /// </summary>
    public sealed class FileSystemStorage
    {
        private readonly string _folderPath;
        private const string FileExtension = ".sav";

        public FileSystemStorage(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Путь к папке не задан.", nameof(folderPath));

            _folderPath = folderPath;

            // Создаём папку, если её ещё нет
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        // -----------------------------------------------------------------
        // Публичный API
        // -----------------------------------------------------------------

        public void Write(string key, byte[] bytes)
        {
            ValidateKey(key);
            File.WriteAllBytes(GetFilePath(key), bytes);
        }

        public byte[] Read(string key)
        {
            ValidateKey(key);
            string path = GetFilePath(key);

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Файл сохранения не найден: {path}", path);

            return File.ReadAllBytes(path);
        }

        public bool Exists(string key)
        {
            ValidateKey(key);
            return File.Exists(GetFilePath(key));
        }

        public void Delete(string key)
        {
            ValidateKey(key);
            string path = GetFilePath(key);

            if (File.Exists(path))
                File.Delete(path);
        }

        // -----------------------------------------------------------------
        // Приватные вспомогательные методы
        // -----------------------------------------------------------------

        private string GetFilePath(string key) =>
            Path.Combine(_folderPath, key + FileExtension);

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Ключ не может быть пустым.", nameof(key));
        }
    }
}
