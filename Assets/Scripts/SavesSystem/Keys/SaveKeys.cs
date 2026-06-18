namespace SaveSystem.Keys
{
    /// <summary>
    /// Заранее заданные ключи для всех сохраняемых структур.
    ///
    /// Ключ — это имя файла (без расширения), под которым данные
    /// будут записаны на диск.
    ///
    /// Правило: никогда не переименовывайте существующие константы —
    /// иначе старые файлы сохранений перестанут подхватываться.
    /// Если структура данных изменилась, добавьте новую константу.
    /// </summary>
    public static class SaveKeys
    {
        /// <summary>Весь снапшот игры одним файлом.</summary>
        public const string GameData = "game_data";

        // При необходимости разбить на независимые файлы раскомментируйте:
        // public const string PlayerProgress = "player_progress";
        // public const string PlayerState    = "player_state";
    }
}
