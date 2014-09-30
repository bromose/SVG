using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Svg
{
    /// <summary>
    /// Represents a System.Uri in an SVG document.  This was created to handle very long data URI values
    /// </summary>
    [TypeConverter(typeof(SvgUriConverter))]
    public sealed class SvgUri
    {
        string m_mime;
        byte[] m_data;
        Uri m_uri;
        const string DATA_HEADER = "data:";
        const string DATA_BASE64 = "base64";
        /// <summary>
        /// Creates a new empty instance
        /// </summary>
        public SvgUri()
            : this("")
        {
        }
        /// <summary>
        /// Create a new instance based on the given URI value
        /// </summary>
        /// <param name="uri"></param>
        public SvgUri(string uri)
        {
            if (!TryDecodeDataUri(uri, out m_mime, out m_data))
                Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out m_uri);
        }

        public override string ToString()
        {
            if (m_uri != null)
                return m_uri.ToString();
            return DATA_HEADER + m_mime + ';' + DATA_BASE64 + Convert.ToBase64String(m_data);
        }
        /// <summary>
        /// Attempts to download the contents at the referenced URI
        /// </summary>
        /// <param name="mime">The content type of what is downloaded</param>
        /// <returns></returns>
        public Stream DownloadUrl(out string mime)
        {
            if (m_data != null)
            {
                mime = m_mime;
                return new MemoryStream(m_data);
            }
            else
            {
                var httpRequest = WebRequest.Create(m_uri);
                using (WebResponse webResponse = httpRequest.GetResponse())
                {
                    mime = webResponse.Headers[HttpResponseHeader.ContentType];
                    var ms = new MemoryStream();
                    webResponse.GetResponseStream().CopyTo(ms);
                    return ms;
                }
            }
        }
        /// <summary>
        /// Is the given uri a data URI? If so extract the mime type and encoded data
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="mime"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryDecodeDataUri(string uri, out string mime, out byte[] data)
        {
            if (uri == null)
                goto no_go;
            if (!uri.StartsWith(DATA_HEADER))
                goto no_go;
            int semicolon = uri.IndexOf(';');
            if (semicolon < 0)
                goto no_go;
            mime = uri.Substring(DATA_HEADER.Length, semicolon - DATA_HEADER.Length);
            int comma = uri.IndexOf(',', semicolon);
            if (comma < 0)
                goto no_go;
            string encoding = uri.Substring(semicolon + 1, comma - semicolon - 1);
            if (encoding != DATA_BASE64)
                goto no_go;
            string data_str = uri.Substring(comma + 1);
            data = Convert.FromBase64String(data_str);
            return true;
        no_go:
            mime = null;
            data = null;
            return false;
        }
    }
}
