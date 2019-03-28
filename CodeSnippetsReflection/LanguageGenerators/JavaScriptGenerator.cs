﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSnippetsReflection.LanguageGenerators
{
    public static class JavaScriptGenerator
    {
        /// <summary>
        /// Formulates the requested Graph snippets and returns it as string for JavaScript
        /// </summary>
        /// <param name="snippetModel">Model of the Snippets info <see cref="SnippetModel"/></param>
        /// <param name="languageExpressions">The language expressions to be used for code Gen</param>
        /// <returns>String of the snippet in Javascript code</returns>
        public static string GenerateCodeSnippet(SnippetModel snippetModel , LanguageExpressions languageExpressions)
        {
            try
            {
                var snippetBuilder = new StringBuilder();
                //setup the auth snippet section
                snippetBuilder.Append("// Some callback function\n");
                snippetBuilder.Append("const authProvider: AuthProvider = (callback: AuthProviderCallback) => { \n");
                snippetBuilder.Append("\t// Your logic for getting and refreshing accessToken \n");
                snippetBuilder.Append("\t// Error should be passed in case of error while authenticating \n");
                snippetBuilder.Append("\t// accessToken should be passed upon successful authentication \n");
                snippetBuilder.Append("\tcallback(error, accessToken);\n};\n\n");
                snippetBuilder.Append("let options: Options = {\n");
                snippetBuilder.Append("\t\tauthProvider,\n};\n\n");
                //init the client
                snippetBuilder.Append("const client = Client.init(options);\n\n");

                if (snippetModel.Method == HttpMethod.Get)
                {
                    snippetBuilder.Append($"var {snippetModel.ResponseVariableName} = client.api('{snippetModel.Path}')");
                    //append beta
                    snippetBuilder.Append(BetaSectionString(snippetModel.ApiVersion));
                    snippetBuilder.Append(CommonGenerator.GenerateQuerySection(snippetModel, languageExpressions));
                    //append footer
                    snippetBuilder.Append("\n\t.get();");
                    
                }
                else if (snippetModel.Method == HttpMethod.Post)
                {
                    var name = "";
                    // create variable to send out if we have a body
                    if (!string.IsNullOrEmpty(snippetModel.RequestBody))
                    {
                        name = snippetModel.ResponseVariableName;
                        snippetBuilder.Append(JavascriptGenerateObjectFromJson(snippetModel.RequestBody,name));
                        var objectType = CommonGenerator.GetClassNameFromIdentifier(snippetModel.Segments.Last() , new List<string> { snippetModel.Segments.Last().Identifier });
                        //we need to split the string and get last item
                        //eg microsoft.graph.data => data
                        objectType = objectType.Split(".").Last();
                        //parameter for the post.
                        name = $"{{{objectType} : { name }}}";
                    }
                             
                    snippetBuilder.Append($"client.api('{snippetModel.Path}')");
                    //append beta
                    snippetBuilder.Append(BetaSectionString(snippetModel.ApiVersion));
                    snippetBuilder.Append($".post({name});");

                }
                else
                {
                    throw new NotImplementedException("HTTP method not implemented for Javascript");
                }

                return snippetBuilder.ToString();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Return string to signifying the endpoint to be used in the snippet
        /// </summary>
        /// <param name="apiVersion">Api version of the request</param>
        /// <returns>String of the snippet in JS code</returns>
        private static string BetaSectionString(string apiVersion)
        {
            return apiVersion.Equals("beta") ? "\n\t.version('beta')" : "";
        }

        /// <summary>
        /// Generate a valid declaration of a json object from the json string. This essentially involves stripping out the quotation marks 
        /// out of the json keys
        /// </summary>
        /// <param name="jsonBody">Json body to generate object from</param>
        /// <param name="variableName">name of variable to use</param>
        /// <returns>String of the snippet of the json object declaration in JS code</returns>
        private static string JavascriptGenerateObjectFromJson(string jsonBody , string variableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            //remove the quotation marks from the JSON keys
            const string pattern = "\"(.*?) *\":";
            var javascriptObject = Regex.Replace(jsonBody, pattern, "$1:");
            stringBuilder.Append($"const {variableName} = {javascriptObject};");
            stringBuilder.Append("\r\n\r\n");
            return stringBuilder.ToString();
        }
    }

    internal class JavascriptExpressions : LanguageExpressions
    {
        public override string FilterExpression => "\n\t.filter('{0}')"; 
        public override string SearchExpression => "\n\t.search('{0}')"; 
        public override string ExpandExpression => "\n\t.expand('{0}')"; 
        public override string SelectExpression => "\n\t.select('{0}')"; 
        public override string OrderByExpression => "\n\t.orderby('{0}')"; 
        public override string SkipExpression => "\n\t.skip('{0}')"; 
        public override string SkipTokenExpression  => "\n\t.skiptoken('{0}')"; 
        public override string TopExpression => "\n\t.top('{0}')"; 

        public override string FilterExpressionDelimiter => ",";

        public override string ExpandExpressionDelimiter => ",";

        public override string SelectExpressionDelimiter => ",";

        public override string OrderByExpressionDelimiter => " ";

        public override string HeaderExpression => "\n\t.header('{0}','{1}')";
    }
}
