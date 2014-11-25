using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Svg
{
    /// <summary>
    /// A class that provides a context for creating/opening SVG documents.
    /// </summary>
    public sealed class SvgBuilder
    {
        #region Member Vars
        const string svgNS = "http://www.w3.org/2000/svg";
        /// <summary>
        /// A list of available types that can be used when creating an <see cref="SvgElement"/>.
        /// </summary>
        static Dictionary<string, ElementInfo> AvailableElements;
        static Dictionary<Type, Dictionary<string, PropertyDescriptorCollection>> _propertyDescriptors = new Dictionary<Type, Dictionary<string, PropertyDescriptorCollection>>();
        readonly Func<SvgDocument> m_ctor;
        readonly Dictionary<string, FontFamily> m_knownFonts;
        public static readonly SvgBuilder Default;
        #endregion

        #region Ctor
        static SvgBuilder()
        {
            AvailableElements = new Dictionary<string, ElementInfo>();
            var svgTypes = from t in typeof(SvgDocument).Assembly.GetExportedTypes()
                           where t.GetCustomAttributes(typeof(SvgElementAttribute), true).Length > 0
                           && t.IsSubclassOf(typeof(SvgElement))
                           select new ElementInfo { ElementName = ((SvgElementAttribute)t.GetCustomAttributes(typeof(SvgElementAttribute), true)[0]).ElementName, ElementType = t };
            foreach (var type in svgTypes)
                AvailableElements[type.ElementName] = type;
            AvailableElements["svg"] = new ElementInfo("svg", typeof(SvgDocument));
            Default = new SvgBuilder();
        }

        public SvgBuilder(Func<SvgDocument> ctor)
            : this()
        {
            m_ctor = ctor;
        }

        public SvgBuilder()
        {
            m_ctor = CreateDocument;
            m_knownFonts = System.Drawing.FontFamily.Families.ToDictionary(fn => fn.Name);
        }
        #endregion

        #region Font Methods/Events
        /// <summary>
        /// The event raised when a font family is needed.  This enables support for fonts that are not locally installed.
        /// </summary>
        public event EventHandler<FontFamilyLookupArgs> FontFamilyLookup;
        /// <summary>
        /// Gets a FontFamily for a given font family name
        /// </summary>
        /// <param name="name">The name of the FontFamily to quire</param>
        /// <returns>A newly created FontFamily based on the given name</returns>
        public FontFamily GetFontFamily(string name)
        {
            FontFamily family;
            // Split font family list on "," and then trim start and end spaces and quotes.
            var fontParts = name.Split(new[] { ',' }).Select(fontName => fontName.Trim(new[] { '"', ' ', '\'' }));
            // Check known fonts
            foreach (var part in fontParts)
                if (m_knownFonts.TryGetValue(part, out family))
                    return family;

            if (FontFamilyLookup != null)
                foreach (var part in fontParts)
                {
                    try
                    {
                        var e = new FontFamilyLookupArgs(part);
                        FontFamilyLookup(this, e);
                        if (e.FontFamily != null)
                        {
                            m_knownFonts[e.FontFamily.Name] = e.FontFamily;
                            return e.FontFamily;
                        }
                    }
                    catch { }
                }
            // No valid font family found from the list requested.
            return new FontFamily(SvgText.DefaultFontFamily);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ValidateFontFamily(string name)
        {
            return GetFontFamily(name).Name;
        }
        #endregion

        #region Open Methods
        public SvgDocument Open(string contents)
        {
            using (var ms = new MemoryStream(Encoding.Default.GetBytes(contents)))
                return Open(ms);
        }

        public SvgDocument Open(Stream contents)
        {
            return Open(contents, null);
        }

        public SvgDocument OpenPath(string filename)
        {
            using (var fs = File.Open(filename, FileMode.Open))
                return Open(fs, null);
        }

        public SvgDocument Open(Stream stream, Dictionary<string, string> entities)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            //Trace.TraceInformation("Begin Read");

            using (var reader = new SvgTextReader(stream, entities))
            {
                var elementStack = new Stack<SvgElement>();
                var value = new StringBuilder();
                bool elementEmpty;
                SvgElement element = null;
                SvgElement parent;
                SvgDocument svgDocument = null;
                reader.XmlResolver = new SvgDtdResolver();
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    try
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                // Does this element have a value or children
                                // (Must do this check here before we progress to another node)
                                elementEmpty = reader.IsEmptyElement;
                                // Create element
                                if (elementStack.Count > 0)
                                {
                                    element = CreateElement(reader, svgDocument);
                                }
                                else
                                {
                                    svgDocument = CreateDocument(reader);
                                    element = svgDocument;
                                }

                                // Add to the parents children
                                if (elementStack.Count > 0)
                                {
                                    parent = elementStack.Peek();
                                    if (parent != null && element != null)
                                        parent.Children.Add(element);
                                }

                                // Push element into stack
                                elementStack.Push(element);

                                // Need to process if the element is empty
                                if (elementEmpty)
                                {
                                    goto case XmlNodeType.EndElement;
                                }

                                break;
                            case XmlNodeType.EndElement:

                                // Pop the element out of the stack
                                element = elementStack.Pop();

                                if (value.Length > 0 && element != null)
                                {
                                    element.Content = value.ToString();

                                    // Reset content value for new element
                                    value.Clear();
                                }
                                break;
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Text:
                                value.Append(reader.Value);
                                break;
                        }
                    }
                    catch (Exception exc)
                    {
                        Trace.TraceError(exc.Message);
                    }
                }

                //Trace.TraceInformation("End Read");
                return svgDocument;
            }
        }
        /// <summary>
        /// Given the SVG/XML fragment return a fully populated SVG node.  The returned node is not added to the given document
        /// </summary>
        /// <param name="document">The document context to parse the in content in</param>
        /// <param name="fragment">The SVG/XML formatted string to parse</param>
        /// <param name="entities">Optional dictionary to resolve entities. May be null.</param>
        /// <returns></returns>
        public SvgElement[] ParseFragment(SvgDocument document, string fragment, Dictionary<string, string> entities)
        {
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("svg", svgNS);

            XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

            using (var reader = new SvgTextReader(fragment, XmlNodeType.Element, context, entities))
            {
                var elements = new List<SvgElement>();
                var elementStack = new Stack<SvgElement>();
                var value = new StringBuilder();
                bool elementEmpty;
                SvgElement element = null;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // Does this element have a value or children
                            // (Must do this check here before we progress to another node)
                            elementEmpty = reader.IsEmptyElement;
                            // Create element
                            element = CreateElement(reader, document);

                            // Add to the parents children
                            if (elementStack.Count > 0)
                            {
                                var parent = elementStack.Peek();
                                if (parent != null && element != null)
                                    parent.Children.Add(element);
                            }
                            else
                            {
                                elements.Add(element);
                            }

                            // Push element into stack
                            elementStack.Push(element);

                            // Need to process if the element is empty
                            if (elementEmpty)
                            {
                                goto case XmlNodeType.EndElement;
                            }

                            break;
                        case XmlNodeType.EndElement:

                            // Pop the element out of the stack
                            element = elementStack.Pop();

                            if (value.Length > 0 && element != null)
                            {
                                element.Content = value.ToString();
                                // Reset content value for new element
                                value.Clear();
                            }
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            value.Append(reader.Value);
                            break;
                    }
                }
                return elements.ToArray();
            }
        }
        #endregion

        #region Create Element Methods
        /// <summary>
        /// Creates an <see cref="SvgElement"/> from the current node in the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlTextReader"/> containing the node to parse into a subclass of <see cref="SvgElement"/>.</param>
        /// <param name="document">The <see cref="SvgDocument"/> that the created element belongs to.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> and <paramref name="document"/> parameters cannot be <c>null</c>.</exception>
        SvgElement CreateElement(XmlTextReader reader, SvgDocument document)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return CreateElement(reader, false, document);
        }

        static SvgDocument CreateDocument()
        {
            return new SvgDocument();
        }

        /// <summary>
        /// Creates an <see cref="SvgDocument"/> from the current node in the specified <see cref="XmlTextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlTextReader"/> containing the node to parse into an <see cref="SvgDocument"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> parameter cannot be <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The CreateDocument method can only be used to parse root &lt;svg&gt; elements.</exception>
        SvgDocument CreateDocument(XmlTextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (reader.LocalName != "svg")
            {
                throw new InvalidOperationException("The CreateDocument method can only be used to parse root <svg> elements.");
            }

            var doc = (SvgDocument)CreateElement(reader, true, null);
            return doc;
        }

        SvgElement CreateElement(XmlTextReader reader, bool fragmentIsDocument, SvgDocument document)
        {
            SvgElement createdElement = null;
            string elementName = reader.LocalName;
            //string elementNS = reader.NamespaceURI;

            //Trace.TraceInformation("Begin CreateElement: {0}", elementName);
            //ARES - this caused fragment parsing to fail, and I can't see a reason to worry about this here since we only create elements from AvailableElements
            //if (elementNS == svgNS)
            //{
            if (elementName == "svg")
            {
                createdElement = (fragmentIsDocument) ? CreateDocument() : new SvgFragment();
            }
            else
            {
                ElementInfo validType;
                if (AvailableElements.TryGetValue(elementName, out validType) && validType != null)
                {
                    createdElement = (SvgElement)Activator.CreateInstance(validType.ElementType);
                }
            }

            if (createdElement != null)
            {
                createdElement.SvgBuilder = this;
                SetAttributes(createdElement, reader, document);
            }
            //}

            //Trace.TraceInformation("End CreateElement");

            return createdElement;
        }

        void SetAttributes(SvgElement element, XmlTextReader reader, SvgDocument document)
        {
            //Trace.TraceInformation("Begin SetAttributes");

            string[] styles = null;
            string[] style = null;
            int i = 0;

            while (reader.MoveToNextAttribute())
            {
                // Special treatment for "style"
                if (reader.LocalName.Equals("style"))
                {
                    styles = reader.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    for (i = 0; i < styles.Length; i++)
                    {
                        if (!styles[i].Contains(":"))
                        {
                            continue;
                        }

                        style = styles[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        SetPropertyValue(element, style[0].Trim(), style[1].Trim(), document);
                    }

                    continue;
                }

                SetPropertyValue(element, reader.LocalName, reader.Value, document);
            }

            //Trace.TraceInformation("End SetAttributes");
        }

        static void SetPropertyValue(SvgElement element, string attributeName, string attributeValue, SvgDocument document)
        {
            var elementType = element.GetType();

            PropertyDescriptorCollection properties;
            if (_propertyDescriptors.Keys.Contains(elementType))
            {
                if (_propertyDescriptors[elementType].Keys.Contains(attributeName))
                {
                    properties = _propertyDescriptors[elementType][attributeName];
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(elementType, new[] { new SvgAttributeAttribute(attributeName) });
                    _propertyDescriptors[elementType].Add(attributeName, properties);
                }
            }
            else
            {
                properties = TypeDescriptor.GetProperties(elementType, new[] { new SvgAttributeAttribute(attributeName) });
                _propertyDescriptors.Add(elementType, new Dictionary<string, PropertyDescriptorCollection>());

                _propertyDescriptors[elementType].Add(attributeName, properties);
            }

            if (properties.Count > 0)
            {
                PropertyDescriptor descriptor = properties[0];

                try
                {
                    descriptor.SetValue(element, descriptor.Converter.ConvertFrom(document, CultureInfo.InvariantCulture, attributeValue));
                }
                catch
                {
                    Trace.TraceWarning(string.Format("Attribute '{0}' cannot be set - type '{1}' cannot convert from string '{2}'.", attributeName, descriptor.PropertyType.FullName, attributeValue));
                }
            }
            else
            {
                //check for namespace declaration in svg element
                if (string.Equals(element.ElementName, "svg", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(attributeName, "xmlns", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "xlink", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "xmlns:xlink", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(attributeName, "version", StringComparison.OrdinalIgnoreCase))
                    {
                        //nothing to do
                    }
                    else
                    {
                        //attribute is not a svg attribute, store it in custom attributes
                        element.CustomAttributes[attributeName] = attributeValue;
                    }
                }
                else
                {
                    //attribute is not a svg attribute, store it in custom attributes
                    element.CustomAttributes[attributeName] = attributeValue;
                }
            }
        }
        #endregion

        #region Edit Helpers
        public static void EditOffset(ref PointF loc, float dx, float dy)
        {
            loc = new PointF(loc.X + dx, loc.Y + dy);
        }
        public static void EditScale(ref PointF loc, float scale)
        {
            loc = new PointF(loc.X * scale, loc.Y * scale);
        }
        #endregion

        #region Types
        /// <summary>
        /// Contains information about a type inheriting from <see cref="SvgElement"/>.
        /// </summary>
        [DebuggerDisplay("{ElementName}, {ElementType}")]
        sealed class ElementInfo
        {
            /// <summary>
            /// Gets the SVG name of the <see cref="SvgElement"/>.
            /// </summary>
            public string ElementName { get; set; }
            /// <summary>
            /// Gets the <see cref="Type"/> of the <see cref="SvgElement"/> subclass.
            /// </summary>
            public Type ElementType { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementInfo"/> struct.
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <param name="elementType">Type of the element.</param>
            public ElementInfo(string elementName, Type elementType)
            {
                this.ElementName = elementName;
                this.ElementType = elementType;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementInfo"/> class.
            /// </summary>
            public ElementInfo()
            {
            }
        }
        #endregion
    }
}
