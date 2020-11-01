using System;
using System.Collections.Generic;
using Adriva.Common.Core;

namespace Adriva.Web.Controls.Abstractions
{
    public static class RendererUtilities
    {
        public static string GenerateInitializerScript(IControlOutputContext context, string optimizationContextName, string startupScript, string postStartupScript = null)
        {
            if (null != startupScript && !startupScript.EndsWith(';')) startupScript = startupScript + ";";

            string initializerScript = $"function initialize{context.Id}(){{ {startupScript}{postStartupScript} }};";
            string loaderScript = "if (adriva && adriva.optimization && adriva.optimization.loader && adriva.optimization.loader.hasAssets){"
                                    + $"if (adriva.optimization.loader.isReady('{optimizationContextName}')) {{ initialize{context.Id}(); }}"
                                    + $"else {{ document.addEventListener('contextReady', function(e){{ if('{optimizationContextName}'===e.detail){{ initialize{context.Id}(); }}; }}); }}"
                                + "}"
                                + "else {"
                                    + $"if('complete'===document.readyState) {{ initialize{context.Id}(); }}"
                                    + $"else {{ window.addEventListener('load', initialize{context.Id}); }}"
                                + "} ";

            return $"<script>{initializerScript}{Environment.NewLine}{loaderScript}</script>";
        }

        public static string GenerateWrappedScriptCall(string inlineScript, int parameterCount, out string functionName)
        {
            if (string.IsNullOrWhiteSpace(inlineScript))
            {
                functionName = null;
                return null;
            }

            functionName = "fn_" + Utilities.GetRandomId(8);
            List<string> argumentsList = new List<string>();
            for (int loop = 0; loop < parameterCount; loop++)
            {
                argumentsList.Add($"arguments[{loop}]");
            }

            return $"function {functionName}(){{ return ({inlineScript})({string.Join(",", argumentsList)});}}";
        }

        public static string GenerateWrappedScriptCall(string inlineScript, string[] parameters, out string functionName)
        {
            if (string.IsNullOrWhiteSpace(inlineScript))
            {
                functionName = null;
                return null;
            }

            if (null == parameters) parameters = Array.Empty<string>();

            functionName = "fn_" + Utilities.GetRandomId(8);
            List<string> argumentsList = new List<string>();

            for (int loop = 0; loop < parameters.Length; loop++)
            {
                argumentsList.Add($"arguments[{loop}]");
            }

            return $"function {functionName}({string.Join(',', parameters)}){{ return ({inlineScript})({string.Join(",", argumentsList)});}}";
        }
    }
}