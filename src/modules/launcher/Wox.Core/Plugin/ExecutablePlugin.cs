// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Wox.Plugin;

namespace Wox.Core.Plugin
{
    internal class ExecutablePlugin : JsonRPCPlugin
    {
        private readonly ProcessStartInfo _startInfo;

        public override string SupportedLanguage { get; set; } = AllowedLanguage.Executable;

        public ExecutablePlugin(string filename)
        {
            _startInfo = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
        }

        protected override string ExecuteQuery(Query query)
        {
            JsonRPCServerRequestModel request = new JsonRPCServerRequestModel(new object[] { query.Search })
            {
                Method = "query",
            };

            _startInfo.Arguments = $"\"{request}\"";

            return Execute(_startInfo);
        }

        protected override string ExecuteCallback(JsonRPCRequestModel rpcRequest)
        {
            _startInfo.Arguments = $"\"{rpcRequest}\"";
            return Execute(_startInfo);
        }

        protected override string ExecuteContextMenu(Result selectedResult)
        {
            JsonRPCServerRequestModel request = new JsonRPCServerRequestModel(new object[] { selectedResult.ContextData })
            {
                Method = "contextmenu",
            };

            _startInfo.Arguments = $"\"{request}\"";

            return Execute(_startInfo);
        }
    }
}
