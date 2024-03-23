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

namespace Duplicati.Server.WebServer.RESTMethods
{
    public class LogData : IRESTMethodGET, IRESTMethodDocumented
    {
        public void GET(string key, RequestInfo info)
        {
            if ("poll".Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var input = info.Request.QueryString;
                var level_str = input["level"].Value ?? "";
                var id_str = input["id"].Value ?? "";

                int pagesize;
                if (!int.TryParse(info.Request.QueryString["pagesize"].Value, out pagesize))
                    pagesize = 100;

                pagesize = Math.Max(1, Math.Min(500, pagesize));

                Library.Logging.LogMessageType level;
                long id;

                long.TryParse(id_str, out id);
                Enum.TryParse(level_str, true, out level);

                info.OutputOK(Program.LogHandler.AfterID(id, level, pagesize));
            }
            else
            {

                List<Dictionary<string, object>> res = null;
                Program.DataConnection.ExecuteWithCommand(x =>
                {
                    res = DumpTable(x, "ErrorLog", "Timestamp", info.Request.QueryString["offset"].Value, info.Request.QueryString["pagesize"].Value);
                });

                info.OutputOK(res);
            }
        }


        public static List<Dictionary<string, object>> DumpTable(System.Data.IDbCommand cmd, string tablename, string pagingfield, string offset_str, string pagesize_str)
        {
            var result = new List<Dictionary<string, object>>();

            long pagesize;
            if (!long.TryParse(pagesize_str, out pagesize))
                pagesize = 100;

            pagesize = Math.Max(10, Math.Min(500, pagesize));

            cmd.CommandText = "SELECT * FROM \"" + tablename + "\"";
            long offset = 0;
            if (!string.IsNullOrWhiteSpace(offset_str) && long.TryParse(offset_str, out offset) && !string.IsNullOrEmpty(pagingfield))
            {
                var p = cmd.CreateParameter();
                p.Value = offset;
                cmd.Parameters.Add(p);

                cmd.CommandText += " WHERE \"" + pagingfield + "\" < ?";
            }

            if (!string.IsNullOrEmpty(pagingfield))
                cmd.CommandText += " ORDER BY \"" + pagingfield + "\" DESC";
            cmd.CommandText += " LIMIT " + pagesize.ToString();

            using(var rd = cmd.ExecuteReader())
            {
                var names = new List<string>();
                for(var i = 0; i < rd.FieldCount; i++)
                    names.Add(rd.GetName(i));

                while (rd.Read())
                {
                    var dict = new Dictionary<string, object>();
                    for(int i = 0; i < names.Count; i++)
                        dict[names[i]] = rd.GetValue(i);

                    result.Add(dict);                                    
                }
            }

            return result;
        }

        public string Description { get { return "Retrieves system log data"; } }

        public IEnumerable<KeyValuePair<string, Type>> Types
        {
            get
            {
                return new KeyValuePair<string, Type>[] {
                    new KeyValuePair<string, Type>(HttpServer.Method.Get, typeof(Dictionary<string, string>[])),
                };
            }
        }
    }
}

