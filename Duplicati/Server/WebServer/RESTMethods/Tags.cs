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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Duplicati.Server.WebServer.RESTMethods
{
    public class Tags : IRESTMethodGET, IRESTMethodDocumented
    {
        public void GET(string key, RequestInfo info)
        {
            var r = 
                from n in 
                Serializable.ServerSettings.CompressionModules
                    .Union(Serializable.ServerSettings.EncryptionModules)
                    .Union(Serializable.ServerSettings.BackendModules)
                    .Union(Serializable.ServerSettings.GenericModules)
                    select n.Key.ToLower(CultureInfo.InvariantCulture);

            // Append all known tags
            r = r.Union(from n in Program.DataConnection.Backups select n.Tags into p from x in p select x.ToLower(CultureInfo.InvariantCulture));
            info.OutputOK(r);       
        }

        public string Description { get { return "Gets the list of tags"; } }

        public IEnumerable<KeyValuePair<string, Type>> Types
        {
            get
            {
                return new KeyValuePair<string, Type>[] {
                    new KeyValuePair<string, Type>(HttpServer.Method.Get, typeof(string[])),
                };
            }
        }
    }
}

