// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using Wox.Infrastructure.Logger;
using Wox.Plugin;

namespace Wox.Core.Plugin
{
    /// <summary>
    /// Represent the plugin that using JsonPRC
    /// every JsonRPC plugin should has its own plugin instance
    /// </summary>
    internal abstract class JsonRPCPlugin : IPlugin, IContextMenu
    {
        protected PluginInitContext Context { get; set; }

        public const string JsonRPC = "JsonRPC";

        /// <summary>
        /// Gets or sets the language this JsonRPCPlugin support
        /// </summary>
        public abstract string SupportedLanguage { get; set; }

        protected abstract string ExecuteQuery(Query query);

        protected abstract string ExecuteCallback(JsonRPCRequestModel rpcRequest);

        protected abstract string ExecuteContextMenu(Result selectedResult);

        public List<Result> Query(Query query)
        {
            string output = ExecuteQuery(query);
            try
            {
                return DeserializedResult(output);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception($"Exception when query <{query}>", e, GetType());
                return null;
            }
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            string output = ExecuteContextMenu(selectedResult);
            try
            {
                // This should not hit. If it does it's because Wox shares the same interface for querying context menu items as well as search results. In this case please file a bug.
                // To my knowledge we aren't supporting this JSonRPC commands in Launcher, and am not able to repro this, but I will leave this here for the time being in case I'm proven wrong.
                // We should remove this, or identify and test officially supported use cases and Deserialize this properly.
                // return DeserializedResult(output);
                throw new NotImplementedException();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception($"THIS IS A BUG - Exception on result <{selectedResult}>", e, GetType());
                return null;
            }
        }

        private List<Result> DeserializedResult(string output)
        {
            if (!string.IsNullOrEmpty(output))
            {
                List<Result> results = new List<Result>();

                JsonRPCQueryResponseModel queryResponseModel = JsonConvert.DeserializeObject<JsonRPCQueryResponseModel>(output);
                if (queryResponseModel.Result == null)
                {
                    return null;
                }

                foreach (JsonRPCResult result in queryResponseModel.Result)
                {
                    JsonRPCResult result1 = result;
                    result.Action = c =>
                    {
                        if (result1.JsonRPCAction == null)
                        {
                            return false;
                        }

                        if (!string.IsNullOrEmpty(result1.JsonRPCAction.Method))
                        {
                            // Using InvariantCulture since this is a command line arg
                            if (result1.JsonRPCAction.Method.StartsWith("Wox.", StringComparison.InvariantCulture))
                            {
                                ExecuteWoxAPI(result1.JsonRPCAction.Method.Substring(4), result1.JsonRPCAction.Parameters.ToArray());
                            }
                            else
                            {
                                string actionResponse = ExecuteCallback(result1.JsonRPCAction);
                                JsonRPCRequestModel jsonRpcRequestModel = JsonConvert.DeserializeObject<JsonRPCRequestModel>(actionResponse);

                                // Using InvariantCulture since this is a command line arg
                                if (jsonRpcRequestModel != null
                                    && !string.IsNullOrEmpty(jsonRpcRequestModel.Method)
                                    && jsonRpcRequestModel.Method.StartsWith("Wox.", StringComparison.InvariantCulture))
                                {
                                    ExecuteWoxAPI(jsonRpcRequestModel.Method.Substring(4), jsonRpcRequestModel.Parameters.ToArray());
                                }
                            }
                        }

                        return !result1.JsonRPCAction.DontHideAfterAction;
                    };
                    results.Add(result);
                }

                return results;
            }
            else
            {
                return null;
            }
        }

        private static void ExecuteWoxAPI(string method, object[] parameters)
        {
            MethodInfo methodInfo = PluginManager.API.GetType().GetMethod(method);
            if (methodInfo != null)
            {
                try
                {
                    methodInfo.Invoke(PluginManager.API, parameters);
                }
                catch (Exception)
                {
#if DEBUG
                    {
                        throw;
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Execute external program and return the output
        /// </summary>
        /// <param name="fileName">file to execute</param>
        /// <param name="arguments">args to pass in to that exe</param>
        /// <returns>results</returns>
        protected string Execute(string fileName, string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            return Execute(start);
        }

        protected string Execute(ProcessStartInfo startInfo)
        {
            try
            {
                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        using (var standardOutput = process.StandardOutput)
                        {
                            var result = standardOutput.ReadToEnd();
                            if (string.IsNullOrEmpty(result))
                            {
                                using (var standardError = process.StandardError)
                                {
                                    var error = standardError.ReadToEnd();
                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        Log.Error(error, GetType());

                                        return string.Empty;
                                    }
                                    else
                                    {
                                        Log.Error("Empty standard output and standard error.", GetType());

                                        return string.Empty;
                                    }
                                }
                            }
                            else if (result.StartsWith("DEBUG:", StringComparison.InvariantCulture))
                            {
                                Form form = new Form { TopMost = true };
                                MessageBox.Show(form, result.Substring(6));
                                form.Dispose();

                                return string.Empty;
                            }
                            else
                            {
                                return result;
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Can't start new process", GetType());

                        return string.Empty;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Exception($"Exception for filename <{startInfo.FileName}> with argument <{startInfo.Arguments}>", e, GetType());

                return string.Empty;
            }
        }

        public void Init(PluginInitContext ctx)
        {
            Context = ctx;
        }
    }
}
