using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;
using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.Common.Testing
{
    /// <summary>
    /// Provides a simulated Elasticsearch connection object for use in testing.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RegisterRequestHandlerForType" /> to set simulated Elasticsearch responses.
    /// </remarks>
    public class ElasticsearchInterceptingConnection : IConnection
    {
        /// <summary>
        /// Container for simulated Elasticsearch responses.
        /// </summary>
        public class ResponseData
        {
            /// <summary>
            /// Stream representing the response body.
            /// </summary>
            public Stream Stream { get; set; }

            /// <summary>
            /// The simulated Elasticsearch HTTP status code.  Required if Stream is set.
            /// </summary>
            public int? StatusCode { get; set; }

            /// <summary>
            /// The simulated response MIME type.
            /// </summary>
            public string ResponseMimeType { get; set; }
        }

        private Dictionary<Type, object> _callbackHandlers = new Dictionary<Type, object>();
        private Action<RequestData, object> _defCallbackHandler = null;

        public void Dispose()
        {

        }

        /// <summary>
        /// Register a Request Handler for a given return type.
        /// NOTE: DO NOT REGISTER BOTH A CLASS AND ITS BASE CLASS!!!
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="callback"></param>
        public void RegisterRequestHandlerForType<TReturn>(Action<RequestData, ResponseData> callback)
            where TReturn : class
        {
            Type returnType = typeof(TReturn);
            Type handlerType = null;

            //Loop through the register handlers and see if our type is registered, OR
            //if a base class is registered.
            foreach (Type type in _callbackHandlers.Keys)
            {
                if (returnType == type || returnType.GetTypeInfo().IsSubclassOf(type))
                {
                    handlerType = type;
                    break;
                }
            }

            //If there is no handler, then add one.
            //If there is one, then error out
            if (handlerType == null)
            {
                _callbackHandlers.Add(typeof(TReturn), (object)callback);
            } else
            {
                throw new ArgumentException(
                    String.Format(
                        "There is already a handler defined that would be called for type. Trying to add for: {0}, Already Existing: {1}",
                        returnType.ToString(),
                        handlerType.ToString()
                    ));
            }
        }

        public void RegisterDefaultHandler(Action<RequestData, object> callback)
        {
            if (_defCallbackHandler != null)
                throw new ArgumentException("Cannot add more than one default handler");

            this._defCallbackHandler = callback;
        }

        private void ProcessRequest<TReturn>(RequestData requestData, ResponseData responseData)
            where TReturn : class
        {
            Type returnType = typeof(TReturn);
                bool foundHandler = false;

                // Loop through the register handlers and see if our type is registered, OR
                // if a base class is registered.
                foreach (Type type in _callbackHandlers.Keys)
                {
                    if (returnType == type || returnType.GetTypeInfo().IsSubclassOf(type))
                    {
                        foundHandler = true;

                        Action<RequestData, ResponseData> callback =
                            (Action<RequestData, ResponseData>)_callbackHandlers[typeof(TReturn)];

                        callback(
                            requestData,
                            responseData
                        );

                    break;
                }
            }

            //If we did not find one, then fallback to the default.
            if (!foundHandler && _defCallbackHandler != null)
            {
                foundHandler = true;
                _defCallbackHandler(
                    requestData,
                    responseData
                );
            }

            //If we did not find any, throw an exception
            if (!_callbackHandlers.ContainsKey(typeof(TReturn)) && _defCallbackHandler == null)
                throw new ArgumentOutOfRangeException("There is no callback handler for defined for type, " + typeof(TReturn).ToString());

            //It looks like, based on the code and use of the code, not because of actual commeents, that MadeItToResponse gets set
            //once the Connection was able to get a response from a server.  I am going to set it here, but we may need to update later
            //if we want to test connection failures.
            requestData.MadeItToResponse = true;

            // If the dev writing the test DID provide response data, but DID NOT set a MIME type, we'll just have to
            // assume they meant to set "applicaton/json" since that's what Elasticsearch normally sends back.
            if (String.IsNullOrWhiteSpace(responseData.ResponseMimeType)
                && responseData.StatusCode.HasValue
                && responseData.Stream != null)
            {
                responseData.ResponseMimeType = "application/json";
            }

            // This is much friendlier than the "Attempt to read a closed stream" message that will otherwise occur.
            if ( responseData.Stream != null && !responseData.StatusCode.HasValue)
            {
                throw new ArgumentException("If a response stream is set, a status code must also be set.");
            }

            //Basically all requests, even HEAD requests (e.g. AliasExists) need to have a stream to work correctly.
            //Note, a stream of nothing is still a stream.  So if you did not set a stream, we will do it for you.
            //I am sure this will cause issues when trying to test failures of other kinds...  Good use of 4hrs tracking
            //this stupid issue down.
            if (responseData.Stream == null)
            {
                using (MemoryStream stream = new MemoryStream(new byte[0])) {
                    responseData.Stream = stream;
                }
            }
        }

        TReturn IConnection.Request<TReturn>(RequestData requestData)
        {
            Exception processingException = null;
            ResponseData responseData = new ResponseData();

            try
            {
                this.ProcessRequest<TReturn>(requestData, responseData);
            }
            catch (System.Exception ex)
            {
                processingException = ex;
            }

            return ResponseBuilder.ToResponse<TReturn>(requestData, processingException, responseData.StatusCode, null, responseData.Stream, responseData.ResponseMimeType);
        }

        async Task<TReturn> IConnection.RequestAsync<TReturn>(RequestData requestData, System.Threading.CancellationToken cancellationToken)
        {
            Exception processingException = null;
            ResponseData responseData = new ResponseData();

            try
            {
                this.ProcessRequest<TReturn>(requestData, responseData);
            }
            catch (System.Exception ex)
            {
                processingException = ex;
            }

            return await ResponseBuilder.ToResponseAsync<TReturn>(requestData, processingException, responseData.StatusCode, null, responseData.Stream, responseData.ResponseMimeType, cancellationToken);
        }

        /// <summary>
        /// Helper function to extract the body of a request that would be sent to the Elasticsearch server.
        /// </summary>
        /// <param name="requestData">The request object</param>
        /// <returns>JObject containing the request</returns>
        public JObject GetRequestPost(RequestData requestData)
        {
            //Some requests can have this as null.  That is ok...
            if (requestData.PostData == null)
                return null;

            String postBody = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                requestData.PostData.Write(stream, requestData.ConnectionSettings);
                postBody = Encoding.UTF8.GetString(stream.ToArray());
            }

            return JObject.Parse(postBody);
        }
    }
}
