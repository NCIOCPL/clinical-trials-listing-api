using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;
using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.Common.Testing
{
    public class ElasticsearchInterceptingConnection : IConnection
    {
        private Dictionary<Type, object> _callbackHandlers = new Dictionary<Type, object>();
        private Action<RequestData, object> _defCallbackHandler = null;

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Register a Request Handler for a Given return type.
        /// NOTE: DO NOT REGISTER BOTH A CLASS AND ITS BASE CLASS!!!
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="callback"></param>
        public void RegisterRequestHandlerForType<TReturn>(Action<RequestData, ResponseBuilder<TReturn>> callback)
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

        private void ProcessRequest<TReturn>(RequestData requestData, ResponseBuilder<TReturn> builder)
            where TReturn : class
        {
            Type returnType = typeof(TReturn);
            bool foundHandler = false;

            //Loop through the register handlers and see if our type is registered, OR
            //if a base class is registered.  
            foreach (Type type in _callbackHandlers.Keys)
            {
                if (returnType == type || returnType.GetTypeInfo().IsSubclassOf(type))
                {
                    foundHandler = true;

                    Action<RequestData, ResponseBuilder<TReturn>> callback =
                        (Action<RequestData, ResponseBuilder<TReturn>>)_callbackHandlers[typeof(TReturn)];

                    callback(
                        requestData,
                        builder
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
                    builder
                );
            }

            //If we did not find any, throw an exception
            if (!_callbackHandlers.ContainsKey(typeof(TReturn)) && _defCallbackHandler == null)
                throw new ArgumentOutOfRangeException("There is no callback handler for defined for type, " + typeof(TReturn).ToString());

            //It looks like, based on the code and use of the code, not because of actual commeents, that MadeItToResponse gets set
            //once the Connection was able to get a response from a server.  I am going to set it here, but we may need to update later
            //if we want to test connection failures. 
            requestData.MadeItToResponse = true;

            //Basically all requests, even HEAD requests (e.g. AliasExists) need to have a stream to work correctly.
            //Note, a stream of nothing is still a stream.  So if you did not set a stream, we will do it for you.
            //I am sure this will cause issues when trying to test failures of other kinds...  Good use of 4hrs tracking
            //this stupid issue down.
            if (builder.Stream == null) 
            {
                using (MemoryStream stream = new MemoryStream(new byte[0])) {
                    builder.Stream = stream;
                }
            }
        }

        ElasticsearchResponse<TReturn> IConnection.Request<TReturn>(RequestData requestData)            
        {
            ResponseBuilder<TReturn> builder = new ResponseBuilder<TReturn>(requestData);

            this.ProcessRequest<TReturn>(requestData, builder);

            return builder.ToResponse();
        }

        async Task<ElasticsearchResponse<TReturn>> IConnection.RequestAsync<TReturn>(RequestData requestData, System.Threading.CancellationToken cancellationToken)
        {

            ResponseBuilder<TReturn> builder = new ResponseBuilder<TReturn>(requestData, cancellationToken);

            this.ProcessRequest<TReturn>(requestData, builder);

            return await builder.ToResponseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function to extract the postbody (as JObject) from a request.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public JObject GetRequestPost(RequestData requestData)
        {
            //Some requests can have this as null.  That is ok...
            if (requestData.PostData == null)
                return null;

            String postBody = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {

                //requestData.PostBody
                requestData.PostData.Write(stream, requestData.ConnectionSettings);

                postBody = Encoding.UTF8.GetString(stream.ToArray());

            }

            return JObject.Parse(postBody);
        }
    }
}
