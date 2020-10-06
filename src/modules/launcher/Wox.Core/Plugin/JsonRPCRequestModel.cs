// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* We basically follow the Json-RPC 2.0 spec (http://www.jsonrpc.org/specification) to invoke methods between Wox and other plugins,
 * like python or other self-execute program. But, we added additional infos (proxy and so on) into rpc request. Also, we didn't use the
 * "id" and "jsonrpc" in the request, since it's not so useful in our request model.
 *
 * When execute a query:
 *      Wox -------JsonRPCServerRequestModel--------> client
 *      Wox <------JsonRPCQueryResponseModel--------- client
 *
 * When execute a action (which mean user select an item in reulst item):
 *      Wox -------JsonRPCServerRequestModel--------> client
 *      Wox <------JsonRPCResponseModel-------------- client
 *
 */

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.VisualBasic;

namespace Wox.Core.Plugin
{
    public class JsonRPCRequestModel : JsonRPCModelBase
    {
        public string Method { get; set; }

        public object[] Parameters { get; set; }

        public override string ToString()
        {
            string rpc = string.Empty;
            if (Parameters != null && Parameters.Length > 0)
            {
                string parameters = Parameters.Aggregate("[", (current, o) => current + (GetParameterByType(o) + ","));
                parameters = parameters.Substring(0, parameters.Length - 1) + "]";

                // Using InvariantCulture since this is a command line arg
                rpc = string.Format(CultureInfo.InvariantCulture, @"{{\""method\"":\""{0}\"",\""parameters\"":{1}", Method, parameters);
            }
            else
            {
                // Using InvariantCulture since this is a command line arg
                rpc = string.Format(CultureInfo.InvariantCulture, @"{{\""method\"":\""{0}\"",\""parameters\"":[]", Method);
            }

            return rpc;
        }

        private static string GetParameterByType(object parameter)
        {
            if (parameter == null)
            {
                return "null";
            }

            if (parameter is string)
            {
                // Using InvariantCulture since this is a command line arg
                return string.Format(CultureInfo.InvariantCulture, @"\""{0}\""", ReplaceEscapes(parameter.ToString()));
            }

            if (parameter is int || parameter is float || parameter is double)
            {
                // Using InvariantCulture since this is a command line arg
                return string.Format(CultureInfo.InvariantCulture, @"{0}", parameter);
            }

            if (parameter is bool)
            {
                // Using InvariantCulture since this is a command line arg
                return string.Format(CultureInfo.InvariantCulture, @"{0}", parameter.ToString().ToUpperInvariant());
            }

            return parameter.ToString();
        }

        private static string ReplaceEscapes(string str)
        {
            // Using InvariantCulture since this is a command line arg
            return str.Replace(@"\", @"\\", StringComparison.InvariantCulture) // Escapes in ProcessStartInfo
                .Replace(@"\", @"\\", StringComparison.InvariantCulture) // Escapes itself when passed to client
                .Replace(@"""", @"\\""""", StringComparison.InvariantCulture);
        }
    }
}
