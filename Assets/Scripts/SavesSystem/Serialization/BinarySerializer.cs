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


//using System;
//using System.IO;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
//using UnityEngine;

//namespace SaveSystem.Serialization
//{
//    /// <summary>
//    /// Суррогат для сериализации UnityEngine.Vector2 через BinaryFormatter.
//    /// </summary>
//    public sealed class Vector2SerializationSurrogate : ISerializationSurrogate
//    {
//        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
//        {
//            Vector2 v = (Vector2)obj;
//            info.AddValue("x", v.x);
//            info.AddValue("y", v.y);
//        }

//        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
//        {
//            Vector2 v = (Vector2)obj;
//            v.x = info.GetSingle("x");
//            v.y = info.GetSingle("y");
//            return v;
//        }
//    }

//    /// <summary>
//    /// Суррогат для сериализации UnityEngine.Vector3 (на случай использования в будущем).
//    /// </summary>
//    public sealed class Vector3SerializationSurrogate : ISerializationSurrogate
//    {
//        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
//        {
//            Vector3 v = (Vector3)obj;
//            info.AddValue("x", v.x);
//            info.AddValue("y", v.y);
//            info.AddValue("z", v.z);
//        }

//        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
//        {
//            Vector3 v = (Vector3)obj;
//            v.x = info.GetSingle("x");
//            v.y = info.GetSingle("y");
//            v.z = info.GetSingle("z");
//            return v;
//        }
//    }

//    /// <summary>
//    /// Суррогат для сериализации UnityEngine.Color (на случай использования в будущем).
//    /// </summary>
//    public sealed class ColorSerializationSurrogate : ISerializationSurrogate
//    {
//        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
//        {
//            Color c = (Color)obj;
//            info.AddValue("r", c.r);
//            info.AddValue("g", c.g);
//            info.AddValue("b", c.b);
//            info.AddValue("a", c.a);
//        }

//        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
//        {
//            Color c = (Color)obj;
//            c.r = info.GetSingle("r");
//            c.g = info.GetSingle("g");
//            c.b = info.GetSingle("b");
//            c.a = info.GetSingle("a");
//            return c;
//        }
//    }

//    /// <summary>
//    /// Сериализует данные в бинарный формат (byte[]).
//    /// Поддерживает корректную сериализацию Vector2, Vector3 и Color.
//    /// </summary>
//    public sealed class BinarySerializer
//    {
//        private readonly SurrogateSelector _surrogateSelector;

//        public BinarySerializer()
//        {
//            // Инициализируем селектор суррогатов
//            _surrogateSelector = new SurrogateSelector();
//            var context = new StreamingContext(StreamingContextStates.All);

//            // Регистрируем правила сериализации для типов Unity
//            _surrogateSelector.AddSurrogate(typeof(Vector2), context, new Vector2SerializationSurrogate());
//            _surrogateSelector.AddSurrogate(typeof(Vector3), context, new Vector3SerializationSurrogate());
//            _surrogateSelector.AddSurrogate(typeof(Color), context, new ColorSerializationSurrogate());
//        }

//        public byte[] Serialize<TData>(TData data)
//        {
//            if (data == null)
//                throw new ArgumentNullException(nameof(data));

//            var formatter = new BinaryFormatter();
//            // Назначаем селектор суррогатов перед началом сериализации
//            formatter.SurrogateSelector = _surrogateSelector;

//            using var stream = new MemoryStream();
//            formatter.Serialize(stream, data);
//            return stream.ToArray();
//        }

//        public TData Deserialize<TData>(byte[] bytes)
//        {
//            if (bytes == null || bytes.Length == 0)
//                throw new ArgumentException("Пустой массив байт.", nameof(bytes));

//            var formatter = new BinaryFormatter();
//            // Назначаем селектор суррогатов перед началом десериализации
//            formatter.SurrogateSelector = _surrogateSelector;

//            using var stream = new MemoryStream(bytes);
//            return (TData)formatter.Deserialize(stream);
//        }
//    }
//}