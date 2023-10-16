using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RoboSharp
{
    /// <summary>
    /// Serialize an IEnumerable&lt;IRoboCommand&gt; into/out of XML
    /// </summary>
    public class RoboCommandXmlSerializer : IRoboQueueSerializer
    {

        /// <inheritdoc cref="Append(XElement, IEnumerable{IRoboCommand})"/>
        protected void Append(XElement parent, params IRoboCommand[] commands) => Append(parent, (IEnumerable<IRoboCommand>)commands);

        /// <summary>
        /// Serializes the <paramref name="commands"/> and adds each one as a new node within the <paramref name="parent"/>
        /// </summary>
        /// <param name="parent">The parent element to add the collection of serialized commands into</param>
        /// <param name="commands">the commands to serialize</param>
        protected void Append(XElement parent, IEnumerable<IRoboCommand> commands) => parent.Add(commands.Select(SerializeRoboCommand).ToArray());

        /// <summary>
        /// The factory method used to create the IRoboCommand object during deserialization.
        /// </summary>
        /// <returns>A new <see cref="IRoboCommand"/> object</returns>
        protected virtual IRoboCommand CreateIRoboCommand(string name, CopyOptions copyOptions, LoggingOptions loggingOptions, SelectionOptions selectionOptions, RetryOptions retryOptions)
        {
            return new RoboCommand()
            {
                Name = name,
                CopyOptions = copyOptions,
                LoggingOptions = loggingOptions,
                RetryOptions = retryOptions,
                SelectionOptions = selectionOptions,
            };
        }

        /// <inheritdoc/>
        /// <exception cref="FileNotFoundException"/>
        /// <inheritdoc cref="Deserialize(XElement)"/>
        public virtual IEnumerable<IRoboCommand> Deserialize(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            XDocument doc = XDocument.Load(path);
            return Deserialize(doc.Root);
        }

        /// <summary>
        /// Scan the <paramref name="parent"/> node for children elements named 'IRoboCommand' and parse each one into a new <see cref="RoboCommand"/> object.
        /// </summary>
        /// <param name="parent">The parent node which contains Elements named 'IRoboCommand'</param>
        /// <returns>A collection of <see cref="IRoboCommand"/> objects that were deserialized.</returns>
        public virtual IEnumerable<IRoboCommand> Deserialize(XElement parent)
        {
            List<IRoboCommand> commands = new List<IRoboCommand>();
            foreach (var rootNode in parent.Elements(nameof(IRoboCommand)))
            {
                var Copy = ReadNodes<CopyOptions>(rootNode.Element(nameof(CopyOptions)));
                var Logging = ReadNodes<LoggingOptions>(rootNode.Element(nameof(LoggingOptions)));
                var Retry = ReadNodes<RetryOptions>(rootNode.Element(nameof(RetryOptions)));
                var Selection = ReadNodes<SelectionOptions>(rootNode.Element(nameof(SelectionOptions)));
                commands.Add(CreateIRoboCommand(
                    name: rootNode.Attribute("Name")?.Value ?? string.Empty,
                    copyOptions: Copy,
                    loggingOptions: Logging,
                    selectionOptions: Selection,
                    retryOptions: Retry
                    ));
            }
            return commands;
        }

        /// <summary>
        /// Create a new XmlWriter object to write at the specified path.
        /// </summary>
        protected virtual XmlWriter GetXmlWriter(string path)
        {
            return XmlWriter.Create(path, new System.Xml.XmlWriterSettings()
            {
                Indent = true,
            });
        }

        /// <inheritdoc/>
        /// <remarks>Note: This overwrites the xml file at the specified <paramref name="path"/></remarks>
        /// <inheritdoc cref="System.Xml.XmlWriter.Create(string)"/>
        /// <inheritdoc cref="XDocument.Save(System.Xml.XmlWriter)"/>
        public void Serialize(IEnumerable<IRoboCommand> commands, string path)
        {
            var doc = new XDocument(new XElement("IRoboCommands"));
            Append(doc.Root, commands);
            using (XmlWriter writer = GetXmlWriter(path))
            {
                doc.Save(writer);
                writer.Dispose();
            }
        }

        /// <inheritdoc cref="Serialize(IEnumerable{IRoboCommand}, string)"/>
        public void Serialize(string path, params IRoboCommand[] commands)
            => Serialize(commands, path);

        /// <summary>
        /// Serialize an IRoboCommand into an <see cref="XElement"/>
        /// </summary>
        /// <param name="command">The command to serialize</param>
        /// <returns>An <see cref="XElement"/> tree representing the serialized <paramref name="command"/></returns>
        protected virtual XElement SerializeRoboCommand(IRoboCommand command)
        {
            var root = new XElement(nameof(IRoboCommand));
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

        /// <summary>Name of nodes contained within IEnumerable nodes</summary>
        protected const string CollectionItemXelementName = "Item";
        private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;

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
                    if (TryGetDefaultValue<string>(prop, out string defaultValue) && value.Equals(defaultValue, StringComparison.InvariantCultureIgnoreCase)) continue;
                }
                else if (propType == typeof(bool))
                {
                    bool val = (bool)prop.GetValue(optionsObject);
                    if (TryGetDefaultValue<bool>(prop, out bool defaultValue) && val == defaultValue) continue;
                    if (!val && !createNodeWhenFalse) continue;
                    value = val.ToString();
                }
                else if (propType == typeof(int))
                {
                    int val = (int)prop.GetValue(optionsObject);
                    if (TryGetDefaultValue<int>(prop, out int defaultValue) && val == defaultValue) continue;
                    value = val.ToString();
                }
                else if (propType == typeof(long))
                {
                    long val = (long)prop.GetValue(optionsObject);
                    if (TryGetDefaultValue(prop, out long defaultValue) && val == defaultValue) continue;
                    value = val.ToString();
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

        private static bool TryGetDefaultValue<T>(PropertyInfo prop, out T value)
        {
            value = default;
            var defAttr = prop.GetCustomAttribute(typeof(DefaultValueAttribute));
            if (defAttr is DefaultValueAttribute valueAttr)
            {
                value = (T)valueAttr.Value;
                return true;
            }
            return false;
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
    }
}
