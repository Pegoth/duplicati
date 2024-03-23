// Copyright (C) 2024, The Duplicati Team
// https://duplicati.com, hello@duplicati.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System;
using Duplicati.Server.Serialization;

namespace Duplicati.Server.WebServer
{
    public class BodyWriter : System.IO.StreamWriter, IDisposable
    {
        private readonly HttpServer.IHttpResponse m_resp;
        private readonly string m_jsonp;
        private static readonly object SUCCESS_RESPONSE = new { Status = "OK" };

        // We override the format provider so all JSON output uses US format
        public override IFormatProvider FormatProvider
        {
            get { return System.Globalization.CultureInfo.InvariantCulture; }
        }

        public BodyWriter(HttpServer.IHttpResponse resp, HttpServer.IHttpRequest request)
            : this(resp, request.QueryString["jsonp"].Value)
        {
        }

        public BodyWriter(HttpServer.IHttpResponse resp, string jsonp)
            : base(resp.Body, resp.Encoding)
        {
            m_resp = resp;
            m_jsonp = jsonp;
            if (!m_resp.HeadersSent)
                m_resp.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
        }

        protected override void Dispose (bool disposing)
        {
            if (!m_resp.HeadersSent)
            {
                base.Flush();
                m_resp.ContentLength = base.BaseStream.Length;
                m_resp.Send();
            }
            base.Dispose(disposing);
        }

        public void SetOK()
        {
            m_resp.Reason = "OK";
            m_resp.Status = System.Net.HttpStatusCode.OK;
        }

        public void OutputOK(object result = null)
        {
            SetOK();
            WriteJsonObject(result ?? SUCCESS_RESPONSE);
        }

        public void WriteJsonObject(object o)
        {
            if (!m_resp.HeadersSent)
                m_resp.ContentType = "application/json";

            using(this)
            {
                if (!string.IsNullOrEmpty(m_jsonp))
                {
                    this.Write(m_jsonp);
                    this.Write('(');
                }

                Serializer.SerializeJson(this, o, true);

                if (!string.IsNullOrEmpty(m_jsonp))
                {
                    this.Write(')');
                    this.Flush();
                }
            }
        }
    }

}

