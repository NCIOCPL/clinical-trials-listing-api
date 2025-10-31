# Clinical Trials Information Loader

This directory contains the software for loading dynamic listing data into Elasticsearch to support the clinical trials listing API. For information about For details on the requirements for this loader, please refer to the [original GitHub issue](https://github.com/NCIOCPL/clinical-trials-listing-api/issues/2).

This version has been rewritten to decouple it from dependencies on the CDR system, which is being retired.

## Command Line Options

There are quite a few command-line options. Most are pretty straightforward:
- **auth** (optional comma-separated username and password)
- **dump** (save the concepts and the index data in files)
- **concepts** (locally cached dump of listing info records)
- **debug** (increase level of logging)
- **host** (override Elasticsearch host name)
- **limit** (throttle the number of concepts for testing)
- **sleep** (override maximum number of seconds to sleep on failure)
- **port** (override Elasticsearch port number)
- **test** (write to the file system, not Elasticsearch)
- **verbose** (write progress to the console)
- **groups** (locally cached dump of listing info records)

It takes about two and a half minutes to fetch the approximately 80,000 concepts from the EVS. It takes about a second to assemble those into the groups that we'll load into Elasticsearch, and another three seconds or so to perform that load using the bulk API.

## Optimizations

There are a couple of optimizations you can use during testing and development. If the `--dump` option is used for a run, the script will save three files:
- `../dumps/concepts-TIMESTAMP.json`
- `../dumps/ListingInfo.jsonl`
- `../dumps/TrialTypeInfo.jsonl`

The name of the first file can be passed on the command line as the argument to the `--concepts` option. This will use that cached information instead of talking to the EVS, speeding things up dramatically (and decreasing the likelihood that we'd run up against any service throttling imposed by the EVS, should there be any). And it's just good manners to avoid repeatedly retrieving the same information from their servers.

The other two files are in the bulk load format for Elasticsearch indices, using pairs of JSON-encoded lines. It is possible to pass ListingInfo.jsonl as the argument of the `--groups` command-line option, but there's much less incentive for that optimization, as it only cuts down on the processing time by about a second.

## Test Mode

The script can be run without loading the nodes into Elasticsearch by using the `--test` command-line option, in which case the script will instead write these two files:
- `../dumps/ListingInfo-TIMESTAMP.json`
- `../dumps/TrialTypeInfo-TIMESTAMP.json`

so you can inspect the data which would have been loaded. The `--test` and `--dump` options can be used together.

## Docker Container

There is a `docker-compose.yml` file that can be used to stand up a local Elasticsearch instance for development and testing.
