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
        /// <summary>
        /// Function used to create an <see cref="IRoboCommand"/> object from the deserialized parameters
        /// </summary>
        /// <param name="name"><inheritdoc cref="IRoboCommand.Name"/></param>
        /// <param name="copyOptions">The CopyOptions deserialized from the XML file</param>
        /// <param name="loggingOptions">The LoggingOptions deserialized from the XML file</param>
        /// <param name="retryOptions">The RetryOptions deserialized from the XML file</param>
        /// <param name="selectionOptions">The SelectionOptions deserialized from the XML file</param>
        /// <returns></returns>
        public delegate IRoboCommand CreateIRoboCommandFunc(string name, CopyOptions copyOptions, LoggingOptions loggingOptions, RetryOptions retryOptions, SelectionOptions selectionOptions);

        /// <inheritdoc cref="CreateIRoboCommandFunc"/>
        /// <remarks>This is the default <see cref="CreateIRoboCommandFunc"/> used by the <see cref="RoboCommandXmlSerializer"/></remarks>
        protected static RoboCommand DefaultCreationDelegate(string name, CopyOptions copyOptions, LoggingOptions loggingOptions, RetryOptions retryOptions, SelectionOptions selectionOptions)
            => new RoboCommand(name, null, null, true, null, copyOptions, selectionOptions, retryOptions, loggingOptions, null);

        private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;

        /// <summary>Name of nodes contained within IEnumerable nodes</summary>
        protected const string Item = nameof(Item);

        private readonly CreateIRoboCommandFunc CreationDelegate;

        /// <summary>
        /// Create a new RoboCommandXmlSerializer that serializes / deserializes <see cref="RoboCommand"/> objects
        /// </summary>
        public RoboCommandXmlSerializer() : this(DefaultCreationDelegate) { }

        /// <summary>
        /// Create a new RoboCommandXmlSerializer that uses the provided <paramref name="createIRoboCommandFunc"/> 
        /// to create the IRoboCommand objects during deserialization.
        /// </summary>
        /// <param name="createIRoboCommandFunc">A <see cref="CreateIRoboCommandFunc"/> used to create the IRoboCommands during deserialization</param>
        /// <exception cref="ArgumentNullException"/>
        public RoboCommandXmlSerializer(CreateIRoboCommandFunc createIRoboCommandFunc) 
        {
            CreationDelegate = createIRoboCommandFunc ?? throw new ArgumentNullException(nameof(createIRoboCommandFunc));
        }

        /// <inheritdoc cref="CreateIRoboCommandFunc"/>
        protected virtual IRoboCommand CreateIRoboCommand(string name, CopyOptions copyOptions, LoggingOptions loggingOptions, RetryOptions retryOptions, SelectionOptions selectionOptions)
        {
            return CreationDelegate(name, copyOptions, loggingOptions, retryOptions, selectionOptions);
        }

        /// <summary>
        /// Create a new XmlWriter object to write the xml document to the specified <paramref name="path"/>
        /// </summary>
        protected virtual XmlWriter CreateXmlWriter(string path)
        {
            return XmlWriter.Create(path, new System.Xml.XmlWriterSettings()
            {
                Indent = true,
            });
        }

        /// <inheritdoc/>
        /// <exception cref="FileNotFoundException"/>
        /// <inheritdoc cref="DeserializeCommands(XElement)"/>
        public virtual IEnumerable<IRoboCommand> Deserialize(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            XDocument doc = XDocument.Load(path);
            return DeserializeCommands(doc.Root);
        }

        /// <summary>
        /// Scan the <paramref name="parent"/> node for children elements named 'IRoboCommand' and parse each one into a new <see cref="RoboCommand"/> object.
        /// </summary>
        /// <param name="parent">The parent node which contains Elements named 'IRoboCommand'</param>
        /// <returns>A collection of <see cref="IRoboCommand"/> objects that were deserialized.</returns>
        protected virtual IEnumerable<IRoboCommand> DeserializeCommands(XElement parent)
        {
            return parent.Elements(nameof(IRoboCommand)).Select(DeserializeCommand);
        }

        /// <summary>
        /// Evaluate the <paramref name="IRoboCommandNode"/>'s children, and deserialize it into a new <see cref="IRoboCommand"/>
        /// </summary>
        /// <param name="IRoboCommandNode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        protected virtual IRoboCommand DeserializeCommand(XElement IRoboCommandNode)
        {
            if (IRoboCommandNode is null) throw new ArgumentNullException(nameof(IRoboCommandNode));

            return CreateIRoboCommand(
                name: IRoboCommandNode.Attribute("Name")?.Value ?? string.Empty,
                copyOptions: DeserializeCopyOptions(IRoboCommandNode.Element(nameof(CopyOptions))),
                loggingOptions: DeserializeLoggingOptions(IRoboCommandNode.Element(nameof(LoggingOptions))),
                retryOptions: DeserializeRetryOptions(IRoboCommandNode.Element(nameof(RetryOptions))),
                selectionOptions: DeserializeSelectionOptions(IRoboCommandNode.Element(nameof(SelectionOptions)))
                );
        }

        /// <param name="optionsElement">The element representing the CopyOptions object</param>
        /// <returns>The deserialized CopyOptions object</returns>
        protected virtual CopyOptions DeserializeCopyOptions(XElement optionsElement)
        {
            return DeserializeNode(optionsElement, new CopyOptions());
        }

        /// <param name="optionsElement">The element representing the LoggingOptions object</param>
        /// <returns>The deserialized LoggingOptions object</returns>
        protected virtual LoggingOptions DeserializeLoggingOptions(XElement optionsElement)
        {
            return DeserializeNode(optionsElement, new LoggingOptions());
        }

        /// <param name="optionsElement">The element representing the RetryOptions object</param>
        /// <returns>The deserialized RetryOptions object</returns>
        protected virtual RetryOptions DeserializeRetryOptions(XElement optionsElement)
        {
            return DeserializeNode(optionsElement, new RetryOptions());
        }

        /// <param name="optionsElement">The element representing the SelectionOptions object</param>
        /// <returns>The deserialized SelectionOptions object</returns>
        protected virtual SelectionOptions DeserializeSelectionOptions(XElement optionsElement)
        {
            return DeserializeNode(optionsElement, new SelectionOptions());
        }

        /// <summary>
        /// Apply properties from the supplied <paramref name="xElement"/> to the <paramref name="optionsObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xElement">The parent node, whose children represent the properties of the created object.</param>
        /// <param name="optionsObject">The object to apply the values to</param>
        /// <returns>An <typeparamref name="T"/> object that has the settings provided by the <paramref name="xElement"/></returns>
        protected static T DeserializeNode<T>(XElement xElement, T optionsObject) where T : class, new()
        {
            if (optionsObject is null) optionsObject = new T();
            if (xElement is null) return optionsObject;
            var props = typeof(T).GetProperties(PropertyBindingFlags);
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
                        ?.Elements(Item)
                        ?.Select(item => item.Value)
                        ?.ToArray();
                    if (items is null) continue;
                    if (isPropIEnum)
                        prop.SetValue(optionsObject, items);
                    else if (isPropList)
                        (prop.GetValue(optionsObject) as List<string>).AddRange(items);
                }
                else
                {
                    string strValue = xElement.Element(prop.Name)?.Value;
                    if (strValue is null) continue;
                    if (propType == typeof(string))
                    {
                        prop.SetValue(optionsObject, strValue);
                    }
                    else if (propType == typeof(bool))
                    {
                        if (bool.TryParse(strValue, out bool val))
                            prop.SetValue(optionsObject, val);
                    }
                    else if (propType == typeof(int))
                    {
                        if (int.TryParse(strValue, out var val))
                            prop.SetValue(optionsObject, val);
                    }
                    else if (propType == typeof(long))
                    {
                        if (long.TryParse(strValue, out var val))
                            prop.SetValue(optionsObject, val);
                    }
                }
            }
            return optionsObject;
        }

        /// <inheritdoc/>
        /// <remarks>Note: This overwrites the xml file at the specified <paramref name="path"/></remarks>
        /// <inheritdoc cref="System.Xml.XmlWriter.Create(string)"/>
        /// <inheritdoc cref="XDocument.Save(System.Xml.XmlWriter)"/>
        public virtual void Serialize(IEnumerable<IRoboCommand> commands, string path)
        {
            var doc = new XDocument(new XElement("IRoboCommands"));
            doc.Root.Add(commands.Select(SerializeRoboCommand).ToArray());
            using (XmlWriter writer = CreateXmlWriter(path))
            {
                doc.Save(writer);
                writer.Dispose();
            }
        }

        /// <inheritdoc cref="Serialize(IEnumerable{IRoboCommand}, string)"/>
        public void Serialize(string path, params IRoboCommand[] commands) => Serialize(commands, path);

        /// <summary>
        /// Serialize an IRoboCommand into an <see cref="XElement"/>
        /// </summary>
        /// <param name="command">The command to serialize</param>
        /// <returns>An <see cref="XElement"/> tree representing the serialized <paramref name="command"/></returns>
        protected virtual XElement SerializeRoboCommand(IRoboCommand command)
        {
            var root = new XElement(nameof(IRoboCommand));
            root.SetAttributeValue(nameof(command.Name), command.Name);
            root.SetAttributeValue(nameof(Type), command.GetType().FullName);

            // CopyOptions
            var node = SerializeCopyOptions(command.CopyOptions);
            if (node != null) root.Add(node);

            // Selection Options
            node = SerializeSelectionOptions(command.SelectionOptions);
            if (node != null) root.Add(node);

            // Logging Options
            node = SerializeLoggingOptions(command.LoggingOptions);
            if (node != null) root.Add(node);

            // Retry Options
            node = SerializeRetryOptions(command.RetryOptions);
            if (node != null) root.Add(node);
            return root;
        }

        /// <summary> Serialize the <paramref name="copyOptions"/> object into an <see cref="XElement"/> </summary>
        /// <returns>An <see cref="XElement"/> representing the object, or null if no Xelement was produced.</returns>
        protected virtual XElement SerializeCopyOptions(CopyOptions copyOptions)
        {
            return new XElement(nameof(CopyOptions), CreatePropertyNodes(copyOptions));
        }

        /// <summary> Serialize the <paramref name="loggingOptions"/> object into an <see cref="XElement"/> </summary>
        /// <returns>An <see cref="XElement"/> representing the object, or null if no Xelement was produced.</returns>
        protected virtual XElement SerializeLoggingOptions(LoggingOptions loggingOptions)
        {
            return new XElement(nameof(LoggingOptions), CreatePropertyNodes(loggingOptions));
        }

        /// <summary> Serialize the <paramref name="retryOptions"/> object into an <see cref="XElement"/> </summary>
        /// <returns>An <see cref="XElement"/> representing the object, or null if no Xelement was produced.</returns>
        protected virtual XElement SerializeRetryOptions(RetryOptions retryOptions)
        {
            return new XElement(nameof(RetryOptions), CreatePropertyNodes(retryOptions));
        }

        /// <summary> Serialize the <paramref name="selectionOptions"/> object into an <see cref="XElement"/> </summary>
        /// <returns>An <see cref="XElement"/> representing the object, or null if no Xelement was produced.</returns>
        protected virtual XElement SerializeSelectionOptions(SelectionOptions selectionOptions)
        {
            return new XElement(nameof(SelectionOptions), CreatePropertyNodes(selectionOptions));
        }

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
                        coll.Where(ExtensionMethods.IsNotEmpty).Select(val => new XElement(Item) { Value = val })
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
        /// Try to get the Default Value, as assigned by a <see cref="DefaultValueAttribute"/>, from the <paramref name="prop"/>
        /// </summary>
        /// <returns>TRUE if a default value attribute was located, otherwise FALSE</returns>
        protected static bool TryGetDefaultValue<T>(PropertyInfo prop, out T value)
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
    }
}
