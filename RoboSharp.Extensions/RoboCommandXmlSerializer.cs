using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Serialize an IEnumerable&lt;IRoboCommand&gt; into/out of XML
    /// </summary>
    public class RoboCommandXmlSerializer : IRoboQueueSerializer
    {

        /// <inheritdoc/>
        public virtual IRoboQueueDeserializer Deserialize(string path)
        {
            return new Deserializer(path);
        }

        /// <inheritdoc/>
        public void Serialize(IEnumerable<IRoboCommand> commands, string path)
        {
            var doc = SerializeRoboCommands(commands);
            using (var writer = System.Xml.XmlWriter.Create(path, GetXmlWriterSettings()))
            {
                doc.WriteTo(writer);
                writer.Dispose();
            }
        }

        /// <summary>
        /// Serialize each IRoboCommand via <see cref="SerializeRoboCommand(IRoboCommand)"/>
        /// </summary>
        /// <returns>A new XDocument object</returns>
        protected virtual XDocument SerializeRoboCommands(IEnumerable<IRoboCommand> commands)
        {
            var doc = new XDocument(new XElement("IRoboCommands"));
            foreach (var cmd in commands)
                doc.Root.Add(SerializeRoboCommand(cmd));
            return doc;
        }

        /// <summary>
        /// Serialize an IRoboCommand into an <see cref="XElement"/>
        /// </summary>
        /// <param name="command">The command to serialize</param>
        /// <returns>An <see cref="XElement"/> tree representing the serialized <paramref name="command"/></returns>
        protected virtual XElement SerializeRoboCommand(IRoboCommand command)
        {
            var root = new XElement("IRoboCommand");
            root.SetAttributeValue(nameof(command.Name), command.Name);
            // CopyOptions
            root.Add(new XElement(nameof(command.CopyOptions), CreatePropertyNodes(command.CopyOptions)));
            // Selection Options
            root.Add(new XElement(nameof(command.SelectionOptions), CreatePropertyNodes(command.SelectionOptions)));
            // Logging Options
            root.Add(new XElement(nameof(command.LoggingOptions), CreatePropertyNodes(command.LoggingOptions)));
            // Logging Options
            root.Add(new XElement(nameof(command.RetryOptions), CreatePropertyNodes(command.RetryOptions)));
            return root;
        }

        /// <summary>
        /// Get the XMLWriterSettings object to use when writing the file to disk
        /// </summary>
        protected virtual System.Xml.XmlWriterSettings GetXmlWriterSettings()
        {
            return new System.Xml.XmlWriterSettings()
            {
                Indent = true,
            };
        }


        /// <summary>Name of nodes contained within IEnumerable nodes</summary>
        protected const string CollectionItemXelementName = "Item";
        private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;

        /// <summary>
        /// Read all boolean, integer, and string properties of the specified object, and create <see cref="XElement"/> nodes to represent them.
        /// </summary>
        /// <param name="optionsObject">The object to evaluate</param>
        /// <param name="createNodeWhenFalse">when set false, boolean properties will only be written when their value is true. When set true, always create the node.</param>
        /// <param name="createNodeWhenEmpty">when set false, string properties will only be written when their value is not empty. When set true, always create the node.</param>
        protected static XElement[] CreatePropertyNodes(object optionsObject, bool createNodeWhenFalse = false, bool createNodeWhenEmpty = false)
        {
            var props = optionsObject.GetType().GetProperties(PropertyBindingFlags);
            List<XElement> xElements = new List<XElement>(capacity: props.Length);
            foreach (var prop in props)
            {
                string value = string.Empty;
                Type propType = prop.PropertyType;
                XElement xEl = null;
                if (prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(ObsoleteAttribute)))
                    continue;
                else if (propType == typeof(string))
                {
                    value = prop.GetValue(optionsObject)?.ToString();
                    if (!createNodeWhenEmpty && string.IsNullOrWhiteSpace(value)) continue;
                }
                else if (propType == typeof(bool))
                {
                    bool val = (bool)prop.GetValue(optionsObject);
                    if (!val && !createNodeWhenFalse) continue;
                    value = val.ToString();
                }
                else if (propType == typeof(int) | propType == typeof(long))
                {
                    value = prop.GetValue(optionsObject).ToString();
                }
                else if (propType == typeof(IEnumerable<string>) || propType == typeof(List<string>))
                {
                    IEnumerable<string> coll = prop.GetValue(optionsObject) as IEnumerable<string>;
                    if (coll is null || !coll.Any()) continue;
                    xEl = new XElement(prop.Name);
                    xEl.Add(
                        coll.Select(val => new XElement(CollectionItemXelementName) { Value = val })
                        .ToArray()
                        );
                }
                else
                {
                    continue;
                }
                if (xEl is null)
                {
                    xEl = new XElement(prop.Name);
                    xEl.SetValue(value);
                }
                xElements.Add(xEl);
            }
            return xElements.ToArray();
        }


        /// <summary>
        /// Create a new <typeparamref name="T"/> object, applying properties from the supplied <paramref name="xElement"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xElement">The parent node, whose children represent the properties of the created object.</param>
        /// <returns>A new <typeparamref name="T"/> object</returns>
        protected static T ReadNodes<T>(XElement xElement) where T : new()
        {
            var props = typeof(T).GetProperties(PropertyBindingFlags);
            T obj = new T();
            foreach (var prop in props)
            {
                if (prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(ObsoleteAttribute)))
                    continue;
                Type propType = prop.PropertyType;
                bool isPropIEnum = propType == typeof(IEnumerable<string>);
                bool isPropList = propType == typeof(List<string>);
                if (isPropIEnum || isPropList)
                {
                    
                    var items = xElement
                        .Element(prop.Name)
                        ?.Elements(CollectionItemXelementName)
                        ?.Select(item => item.Value)
                        ?.ToArray();
                    if (items is null) continue;
                    if (isPropIEnum)
                        prop.SetValue(obj, items);
                    else if (isPropList)
                        (prop.GetValue(obj) as List<string>).AddRange(items);
                }
                else
                {
                    string strValue = xElement.Element(prop.Name)?.Value;
                    if (strValue is null) continue;
                    if (propType == typeof(string))
                    {
                        prop.SetValue(obj, strValue);
                    }
                    else if (propType == typeof(bool))
                    {
                        if (bool.TryParse(strValue, out bool val))
                            prop.SetValue(obj, val);
                    }
                    else if (propType == typeof(int))
                    {
                        if (int.TryParse(strValue, out var val))
                            prop.SetValue(obj, val);
                    }
                    else if (propType == typeof(long))
                    {
                        if (long.TryParse(strValue, out var val))
                            prop.SetValue(obj, val);
                    }
                }
            }
            return obj;
        }

        private class Deserializer : Interfaces.IRoboQueueDeserializer
        {
            public Deserializer(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path can not be empty", nameof(path));
                Path = path;
            }
            readonly string Path;

            /// <inheritdoc/>
            public IEnumerable<Interfaces.IRoboCommand> ReadCommands()
            {
                if (!File.Exists(Path)) throw new FileNotFoundException(Path);
                List<IRoboCommand> commands = new List<IRoboCommand>();
                XDocument doc = XDocument.Load(Path);
                foreach(var el in doc.Root.Elements("IRoboCommand"))
                {
                    commands.Add(ReadNodes(el));
                }
                return commands;
            }

            private static RoboCommand ReadNodes(XElement rootNode)
            {
                var Copy = ReadNodes<CopyOptions>(rootNode.Element(nameof(CopyOptions)));
                var Logging = ReadNodes<LoggingOptions>(rootNode.Element(nameof(LoggingOptions)));
                var Retry = ReadNodes<RetryOptions>(rootNode.Element(nameof(RetryOptions)));
                var selection = ReadNodes<SelectionOptions>(rootNode.Element(nameof(SelectionOptions)));
                return new RoboCommand()
                {
                    CopyOptions = Copy,
                    LoggingOptions = Logging,
                    RetryOptions = Retry,
                    SelectionOptions = selection,
                    Name = rootNode.Attribute("Name")?.Value ?? string.Empty
                };
            }

        }

        


    }
}
