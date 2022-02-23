#if DEBUG //TODO Remove IF DEBUG if this is released

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp.Interfaces;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RoboSharp
{
    /// <summary>
    /// Class to Serialize/Deserialize RoboCommand and RoboQueue Objects
    /// </summary>
    internal static class RoboSharpXML
    {
        //TODO: if made public, generate comments for the methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="filePath"></param>
        public static void SaveRoboCommand(RoboCommand command, string filePath) => SerializeClass(command, filePath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="filePath"></param>
        public static void SaveRoboQueue(RoboQueue queue, string filePath) => SerializeClass(queue, filePath);

        private static void SerializeClass<T>(T obj, string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            //TODO: Check valid path
            using (var fileWriter = new StreamWriter(filePath))
            {
                serializer.Serialize(fileWriter, obj);
            }
        }


        /// <summary>
        /// Loads an XML file into a new JobFile object
        /// </summary>
        /// <param name="filePath"><inheritdoc cref="LoadRoboCommand(string)"/></param>
        /// <returns>new JobFile object creation is successful, otherwise returns null.</returns>
        public static JobFile LoadJobFile(string filePath) 
        {
            var tmp = LoadRoboCommand(filePath);
            return tmp == null ? null : new JobFile(tmp);
        }

        /// <summary>
        /// Load an XML file into a new RoboCommand object
        /// </summary>
        /// <param name="filePath">path to some XML file</param>
        /// <returns>new RoboCommand object creation is successful, otherwise returns null.</returns>
        public static RoboCommand LoadRoboCommand(string filePath)
        {
            return Deserialize<RoboCommand>(filePath);
        }

        /// <summary>
        /// Load an XML file into a new RoboQueue object
        /// </summary>
        /// <param name="filePath">path to some XML file</param>
        /// <returns>new RoboQueue object if creation is successful, otherwise returns null.</returns>
        public static RoboQueue LoadRoboQueue(string filePath)
        {
            return Deserialize<RoboQueue>(filePath);
        }

        /// <summary>
        /// Load an XML file into a new object
        /// </summary>
        /// <param name="filePath">path to some XML file</param>
        /// <returns>new object if creation is successful, otherwise returns null.</returns>
        public static T Deserialize<T>(string filePath) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            //TODO: Check valid path
            using (var xmlReader = XmlReader.Create(filePath))
            {
                if (serializer.CanDeserialize(xmlReader))
                {
                    var obj = serializer.Deserialize(xmlReader);
                    return (T)obj;
                }
            }
            return null; //If you get here, return null because deserialization failed
        }

    }
}

#endif