using System;
using System.Collections.Generic;

using Elasticsearch.Net;
using Nest;

using Moq;

namespace NCI.OCPL.Api.Common.Testing
{

    /// <summary>
    /// Tools for mocking elasticsearch clients
    /// </summary>
    public static class ElasticTools {

        /// <summary>
        /// Gets an ElasticClient backed by an InMemoryConnection.  This is used to mock the
        /// JSON returned by the elastic search so that we test the Nest mappings to our models.
        /// </summary>
        /// <param name="testFile"></param>
        /// <param name="requestDataCallback"></param>
        /// <returns></returns>
        public static IElasticClient GetInMemoryElasticClient(string testFile) {

            //Get Response JSON
            byte[] responseBody = TestingTools.GetTestFileAsBytes(testFile);

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            // Setup ElasticSearch stuff using the contents of the JSON file as the client response.
            InMemoryConnection conn = new InMemoryConnection(responseBody);

            var connectionSettings = new ConnectionSettings(pool, conn);

            return new ElasticClient(connectionSettings);
        }

    }
}