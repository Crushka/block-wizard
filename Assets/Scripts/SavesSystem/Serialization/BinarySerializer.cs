using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaveSystem.Serialization
{
    /// <summary>
    /// Сериализует данные в бинарный формат (byte[]).
    /// Бинарный формат компактен и нечитаем без спецсредств —
    /// усложняет ручное редактирование сохранений.
    ///
    /// Требование к типу TData: атрибут [Serializable].
    /// </summary>
    public sealed class BinarySerializer
    {
        public byte[] Serialize<TData>(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }

        public TData Deserialize<TData>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("Пустой массив байт.", nameof(bytes));

            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream(bytes);
            return (TData)formatter.Deserialize(stream);
        }
    }
}
