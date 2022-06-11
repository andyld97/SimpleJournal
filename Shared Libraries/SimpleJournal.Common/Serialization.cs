using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SimpleJournal.Common
{
    /// <summary>
    /// A class which handles the typically tasks of serialization
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// The kind of serialization
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Simple xml-serialization
            /// </summary>
            XML,

            /// <summary>
            /// Byte serialization
            /// Be aware: This class just supports
            /// this mode only when working with fileStreams!
            /// </summary>
            Binary
        }

        #region Read-Methods

        /// <summary>
        /// Reads xmlData and creates a new instance of T
        /// </summary>
        /// <typeparam name="T">The type which you want to deserilize</typeparam>
        /// <param name="xmlData">The xml data</param>
        /// <param name="encoding">The encoding which do you use</param>
        /// <returns></returns>
        public static T ReadString<T>(string xmlData, System.Text.Encoding encoding)
        {
            if (!string.IsNullOrEmpty(xmlData) && encoding != null)
                return Serialization.ReadBytes<T>(encoding.GetBytes(xmlData), Mode.XML);
            else
                return default(T);
        }

        /// <summary>
        /// Reads xmlBytes and creates a new instance of T
        /// </summary>
        /// <typeparam name="T">The type which you want to deserilize</typeparam>
        /// <param name="xmlBytes">The bytes which contain the xmlData</param>
        /// <returns></returns>
        public static T ReadBytes<T>(byte[] xmlBytes, Mode mode)
        {
            if (xmlBytes == null)
                return default(T);

            T instance = default(T);

            try
            {
                if (mode == Mode.XML)
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(xmlBytes, 0, xmlBytes.Length);
                        ms.Position = 0;
                        instance = (T)mySerializer.Deserialize(ms);
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(xmlBytes, 0, xmlBytes.Length);
                        ms.Position = 0;
                        BinaryFormatter bf = new BinaryFormatter();
                        instance = (T)bf.Deserialize(ms);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return instance;
        }

        /// <summary>
        /// Read xmlBytes and returns the instance of type as an object
        /// </summary>
        /// <param name="xmlBytes">The bytes which contain the xmlData</param>
        /// <param name="type">The type you want to deserialize</param>
        /// <returns></returns>
        public static object ReadBytes(byte[] xmlBytes, Type type, Mode mode)
        {
            object instance = null;

            try
            {
                if (mode == Mode.XML)
                {
                    XmlSerializer mySerializer = new XmlSerializer(type);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(xmlBytes, 0, xmlBytes.Length);
                        ms.Position = 0;
                        instance = mySerializer.Deserialize(ms);
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(xmlBytes, 0, xmlBytes.Length);
                        ms.Position = 0;
                        BinaryFormatter bf = new BinaryFormatter();
                        instance = bf.Deserialize(ms);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return instance;
        }

        /// <summary>
        /// Read xmlString and returns instance of type as an object
        /// </summary>
        /// <param name="xmlData">The serialized xml-string</param>
        /// <param name="type">The type you want to deserialize</param>
        /// <param name="encoding">The encoding the string uses</param>
        /// <returns></returns>
        public static object ReadString(string xmlData, Type type, System.Text.Encoding encoding, Mode mode)
        {
            if (string.IsNullOrEmpty(xmlData))
                return null;

            return Serialization.ReadBytes(encoding.GetBytes(xmlData), type, mode);
        }

        /// <summary>
        /// Reads fileStream and creates a new instance of T
        /// </summary>
        /// <typeparam name="T">The type which you want to deserilize</typeparam>
        /// <param name="fileName">The file to read</param>
        /// <param name="mode">The mode <see cref="Mode"/></param>
        /// <param name="additionalTypes">AdditionalTypes which are required for deserializiaton</param>
        /// <returns></returns>
        public static T Read<T>(string fileName, Mode mode = Mode.XML, Type[] additionalTypes = null)
        {
            try
            {
                if (mode == Mode.XML)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), additionalTypes);
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        return (T)xmlSerializer.Deserialize(fileStream);
                    }
                }
                else
                {
                    using (System.IO.FileStream fileStream = new FileStream(fileName, System.IO.FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        T result = (T)bf.Deserialize(fileStream);
                        fileStream.Close();
                        fileStream.Dispose();
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        #endregion

        #region Save-Methods
        /// <summary>
        /// Writes serialized content to your harddisk
        /// </summary>
        /// <typeparam name="T">The type which you want to deserilize</typeparam>
        /// <param name="fileName">The file where you want it to save</param>
        /// <param name="instance">The instance to serliaize</param>
        /// <param name="mode">The mode <see cref="Mode"/></param>
        public static void Save<T>(string fileName, T instance, Mode mode = Mode.XML, Type[] additionalTypes = null)
        {
            if (mode == Mode.XML)
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T), additionalTypes);
                using (StreamWriter myWriter = new StreamWriter(fileName))
                {
                    mySerializer.Serialize(myWriter, instance);
                }
            }
            else
            {
                // Clear file first
                System.IO.File.WriteAllText(fileName, string.Empty);
                using (System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fileStream, instance);
                    fileStream.Close();
                    fileStream.Dispose();
                    bf = null;
                }
            }
        }


        /// <summary>
        /// Serializes the given instance and returns it's xml-content
        /// </summary>
        /// <param name="instance">The instance to serliaize</param>
        /// <param name="encoding">The encoding which do you use</param>
        /// <returns></returns>
        public static string SaveToString<T>(T instance, System.Text.Encoding encoding, Type[] additionalTypes = null)
        {
            byte[] result = Serialization.SaveToBytes<T>(instance, Mode.XML, additionalTypes);
            if (result != null)
                return encoding.GetString(result);

            return string.Empty;
        }

        /// <summary>
        /// Serializes the given instance and returns it's xml-content (as byte-array)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance to serliaize</param>
        /// <returns></returns>
        public static byte[] SaveToBytes<T>(T instance, Mode mode, Type[] additionalTypes = null)
        {
            byte[] serializedContent = null;

            if (mode == Mode.XML)
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T), additionalTypes);
                using (MemoryStream ms = new MemoryStream())
                {
                    mySerializer.Serialize(ms, instance);
                    serializedContent = ms.ToArray();
                }
            }
            else
            {
                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, instance);

                    bf = null;
                    serializedContent = ms.ToArray();
                }

            }
            return serializedContent;
        }

        /// <summary>
        /// Serializes the given instance from the type type to a byte-array (xml-string as byte[])
        /// </summary>
        /// <param name="instance">The instance to serializse</param>
        /// <param name="type">The type of the instance</param>
        /// <returns></returns>
        public static byte[] SaveToBytes(object instance, Type type, Mode mode, Type[] additionalTypes = null)
        {
            byte[] serializedContent = null;
            try
            {
                if (mode == Mode.XML)
                {
                    XmlSerializer mySerializer = new XmlSerializer(type, additionalTypes);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        mySerializer.Serialize(ms, instance);
                        serializedContent = ms.ToArray();
                    }
                }
                else
                {
                    using (System.IO.MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, instance);

                        bf = null;
                        serializedContent = ms.ToArray();
                    }
                }
            }
            catch (Exception)
            { }
            return serializedContent;
        }

        /// <summary>
        /// Serializes the given instance from the type type to a xml-string
        /// </summary>
        /// <param name="instance">The instance to serializse</param>
        /// <param name="type">The type of the instance</param>
        /// <param name="encoding">The encoding used to convert byte-array to string</param>
        /// <returns></returns>
        public static string SaveToString(object instance, Type type, System.Text.Encoding encoding, Type[] additionalTypes = null)
        {
            if (instance != null && type != null && encoding != null)
                return encoding.GetString(Serialization.SaveToBytes(instance, type, Mode.XML, additionalTypes));

            return string.Empty;
        }

        #endregion

    }
}
