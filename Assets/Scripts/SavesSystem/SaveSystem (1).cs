using System;
using SaveSystem.Data;
using SaveSystem.Keys;
using SaveSystem.Serialization;
using SaveSystem.Storage;

namespace SaveSystem
{
    /// <summary>
    /// Главный фасад системы сохранений.
    ///
    /// Связывает три модуля из статьи:
    ///   1. Идентификация  — SaveKeys      (заранее заданный словарь ключей)
    ///   2. Сериализация   — BinarySerializer (бинарный формат)
    ///   3. Хранение       — FileSystemStorage (локальные файлы на диске)
    ///
    /// Пример создания в Unity (например, в GameBootstrapper.cs):
    /// <code>
    ///     string folder = Application.isEditor
    ///         ? Application.dataPath
    ///         : Application.persistentDataPath;
    ///
    ///     var saveSystem = new SaveSystem(folder);
    /// </code>
    /// </summary>
    public sealed class SaveSystem
    {
        private readonly BinarySerializer  _serializer;
        private readonly FileSystemStorage _storage;

        public SaveSystem(string saveFolder)
        {
            _serializer = new BinarySerializer();
            _storage    = new FileSystemStorage(saveFolder);
        }

        // -----------------------------------------------------------------
        // Сохранение / загрузка всего снапшота
        // -----------------------------------------------------------------

        /// <summary>
        /// Сохраняет весь GameData на диск.
        /// Вызывайте при достижении чекпоинта, смерти игрока,
        /// переходе между сценами или по таймеру.
        /// </summary>
        public void Save(GameData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            byte[] bytes = _serializer.Serialize(data);
            _storage.Write(SaveKeys.GameData, bytes);
        }

        /// <summary>
        /// Загружает GameData с диска.
        /// Если сохранения нет — возвращает новый пустой GameData.
        /// Вызывайте при старте игры.
        /// </summary>
        public GameData Load()
        {
            if (!_storage.Exists(SaveKeys.GameData))
                return new GameData();

            try
            {
                byte[] bytes = _storage.Read(SaveKeys.GameData);
                return _serializer.Deserialize<GameData>(bytes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Ошибка загрузки сохранения (файл поврежден): {e.Message}");
                // Если файл сломан, возвращаем новую игру (или можно пытаться загрузить бекап)
                return new GameData();
            }
        }

        /// <summary>
        /// Возвращает true, если на диске уже есть хотя бы одно сохранение.
        /// </summary>
        public bool HasSave() => _storage.Exists(SaveKeys.GameData);

        /// <summary>
        /// Удаляет сохранение с диска (сброс прогресса / новая игра).
        /// </summary>
        public void DeleteSave() => _storage.Delete(SaveKeys.GameData);
    }
}
