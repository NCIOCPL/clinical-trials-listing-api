function fn() {
    var config = {
        apiHost: 'http://localhost:5000'
    };
    var env = karate.env; // get java system property 'karate.env'
    if (java.lang.System.getenv('KARATE_APIHOST')) {
        config.apiHost = java.lang.System.getenv('KARATE_APIHOST');
    }
    return config;
}