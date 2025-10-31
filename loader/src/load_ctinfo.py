#!/usr/bin/env python3

"""Load clinical trial info into Elasticsearch."""

from argparse import ArgumentParser
from datetime import date, datetime, timedelta
from functools import cached_property
from json import JSONDecodeError, dump, dumps, load, loads
from logging import basicConfig, getLogger
from os import chdir
from pathlib import Path
from re import compile as re_compile
from sys import stderr
from time import sleep
from unicodedata import combining, normalize
from warnings import simplefilter
from elasticsearch import Elasticsearch, helpers
from elasticsearch.exceptions import ElasticsearchWarning
from requests import get
from requests.exceptions import RequestException


class Loader:
    """Top-level control for job.

    Options:
      * auth (optional comma-separated username and password)
      * dump (save the concepts and the index data in files)
      * concepts (locally cached dump of listing info records)
      * debug (increase level of logging)
      * host (override Elasticsearch host name)
      * limit (throttle the number of concepts for testing)
      * sleep (override maximum number of seconds to sleep on failure)
      * port (override Elasticsearch port number)
      * test (write to the file system, not Elasticsearch)
      * verbose (write progress to the console)
      * groups (locally cached dump of listing info records)
    """

    USER_CWD = Path(".").resolve()
    chdir(Path(__file__).resolve().parent)
    API = "https://api-evsrest.nci.nih.gov/api/v1/concept/ncit"
    BAD = {"C138195", "C131913"}
    DISEASE = "C7057"
    INTERVENTION = "C1908"
    TOP = DISEASE, INTERVENTION
    LOG = "trial-info"
    FMT = "%(asctime)s [%(levelname)s] %(message)s"
    LABELS = "labels"
    TOKENS = "tokens"
    OVERRIDES = "overrides"
    INFO_DEF = "listing-info.json"
    TRIAL_DEF = "trial-type-info.json"
    INFO = "ListingInfo"
    TRIAL = "TrialTypeInfo"
    INFO_ALIAS = "listinginfov1"
    TRIAL_ALIAS = "trialtypeinfov1"
    LIMIT = 1000000
    MIN_SLEEP = 0.01
    MAX_SLEEP = 500
    MAX_PRETTY_URL_LENGTH = 75
    BATCH_SIZE = 500
    DAYS_TO_KEEP = 5
    INDICES_TO_KEEP = 3
    HOST = "localhost"
    PORT = 9200

    def run(self):
        """Generate JSON records for the API from EVS concepts.

        Fetch and parse the concept records, determining the display
        name for each, and collect concepts into groups sharing the
        same normalized display name.

        We append a handful of records representing hand-curated
        mappings of some of the labels.
        """

        start = datetime.now()
        if self.opts.debug:
            self.logger.setLevel("DEBUG")
        try:
            Path("../dumps").mkdir(exist_ok=True)
            groups = self.groups
            labels = list(self.labels)
            if self.dump:
                self.__dump(groups, labels)
            if self.testing:
                path = f"../dumps/{self.INFO}-{self.stamp}.json"
                with open(path, "w", encoding="utf-8") as fp:
                    dump({self.INFO: groups}, fp, indent=2)
                path = f"../dumps/{self.TRIAL}-{self.stamp}.json"
                with open(path, "w", encoding="utf-8") as fp:
                    dump({self.TRIAL: labels}, fp, indent=2)
            else:
                self.__index(groups, labels)
            if self.verbose:
                stderr.write("done\n")
        except (OSError, IOError, JSONDecodeError, ValueError) as e:
            self.logger.exception("failure")
            self.__alert(e)
        elapsed = datetime.now() - start
        self.logger.info("processing time: %s", elapsed)

    @cached_property
    def auth(self):
        """Optional username, password tuple."""
        return self.opts.auth.split(",", 1) if self.opts.auth else None

    @cached_property
    def concepts(self):
        """Sequence of Concept objects, indexed by code.

        Populated by the `groups` property, which starts with the top-level
        concept records for disease and intervention and fetches the concepts
        recursively.
        """

        # Try for a cached list first.
        if self.opts.concepts:
            path = self.USER_CWD / self.opts.concepts
            if not path.exists():
                path = Path(self.opts.concepts)
            if not path.exists():
                path = Path("../dumps") / self.opts.concepts
            if not path.exists():
                raise RuntimeError(f"{self.opts.concepts!r} not found")

            class CachedConcept:
                """Property method silliness to silence pylint"""

                def __init__(self, code, name):
                    self.__code = code
                    self.__name = name

                @cached_property
                def code(self):
                    """Keep pylint happy"""
                    return self.__code

                @cached_property
                def name(self):
                    """Keep pylint happy"""
                    return self.__name

                @cached_property
                def key(self):
                    """Keep pylint happy"""
                    return self.name.lower()

            with path.open(encoding="utf-8") as fp:
                return [CachedConcept(*values) for values in load(fp)]

        # If there's no cache, talk to the EVS server.
        concepts = {}
        self.__fetch(self.TOP, concepts)
        concepts = concepts.values()

        # Create a cache if so requested.
        if self.dump:
            values = [(c.code, c.name) for c in concepts]
            values = [(int(v[0][1:]), v[0], v[1]) for v in values]
            values = [v[1:] for v in sorted(values)]
            with open(
                f"../dumps/concepts-{self.stamp}.json", "w", encoding="utf-8"
            ) as fp:
                dump(values, fp, indent=2)

        # We now have the list of Concept objects.
        return concepts

    @cached_property
    def es(self):
        """Connection to the Elasticsearch server."""

        opts = {"host": self.host, "port": self.port}
        if self.auth:
            opts["http_auth"] = self.auth
        simplefilter("ignore", ElasticsearchWarning)
        return Elasticsearch([opts])

    @cached_property
    def dump(self):
        """If True, write test data to the file system."""
        return bool(self.opts.dump)

    @cached_property
    def groups(self):
        """Sequence of groups of concepts sharing display names."""

        # Use a locally cached dump if one is specified.
        groups = self.__load_groups_from_cache()
        if groups:
            return groups

        # If no groups cache, assemble the groups from the concepts.
        group_map = {}
        start = datetime.now()
        args = len(self.concepts), datetime.now() - start
        self.logger.info("fetched %d concepts in %s", *args)
        newline = "\n"
        for concept in self.concepts:
            if concept.code in self.BAD:
                continue
            group = group_map.get(concept.key)
            if not group:
                group = group_map[concept.key] = Group(self, concept)
                if self.verbose:
                    stderr.write(f"{newline}\rfound {len(group_map)} groups")
                    newline = ""
            group.codes.append(concept.code)
            if len(group.codes) > 1:
                args = len(group.codes), group.key
                self.logger.debug("%d codes for %r", *args)
        groups = sorted(group_map.values())
        for group in groups:
            matches = {}
            for code in group.codes:
                if code in self.overrides:
                    override = self.overrides[code]
                    codes = str(sorted(override.codes))
                    if codes not in matches:
                        matches[codes] = override
            if len(matches) > 1:
                matches = " and ".join(matches)
                error = f"group {group.key} matches overrides {matches}"
                raise ValueError(error)
            if matches:
                key, override = matches.popitem()
                if override.matched_by:
                    both = f"{group.key} and {override.matched_by}"
                    error = f"override for {codes} matches {both}"
                    raise ValueError(error)
                override.matched_by = group.key
                if override.url == Override.BLANK:
                    group.url = None
                elif override.url != Override.INHERIT:
                    group.url = override.url
                if override.label != Override.INHERIT:
                    group.name = override.label
                    key = group.name.lower()
                    other_group = group_map.get(key)
                    if other_group and other_group is not group:
                        message = f"override results in two {key!r} groups"
                        self.logger.warning(message)
        urls = {}
        for group in groups:
            if group.url in urls:
                other = urls[group.url]
                message = f"{group.url} used by {group.key} and {other}"
                raise RuntimeError(message)
        if self.verbose:
            stderr.write("\n")
        return [group.values for group in groups]

    @cached_property
    def host(self):
        """Name of the Elasticsearch server."""

        host = self.opts.host or self.HOST
        if self.verbose:
            stderr.write(f"connecting to {host}\n")
        self.logger.info("connecting to %s", host)
        return host

    @cached_property
    def info_def(self):
        """Schema for the listing info records."""
        with open(self.INFO_DEF, encoding="utf-8") as fp:
            return load(fp)

    @cached_property
    def labels(self):
        """Map tuples to dictionaries."""

        labels = []
        with open(self.LABELS, encoding="utf-8") as fp:
            for line in fp:
                values = line.strip().split("|")
                url, id_string, label = [value.strip() for value in values]
                labels.append(
                    {
                        "pretty_url_name": url,
                        "id_string": id_string.strip(),
                        "label": label.strip(),
                    }
                )
        return labels

    @cached_property
    def limit(self):
        """Throttle the number of concepts for testing."""
        return int(self.opts.limit) if self.opts.limit else self.LIMIT

    @cached_property
    def logger(self):
        """Used for recording what we do."""
        logger = getLogger(self.LOG)
        basicConfig(filename=f"{self.LOG}.log", level="INFO", format=self.FMT)
        return logger

    @cached_property
    def max_sleep(self):
        """Longest we will wait between fetch failures."""
        return max(self.opts.sleep or self.MAX_SLEEP, self.MIN_SLEEP)

    @cached_property
    def opts(self):
        """Command-line options."""

        parser = ArgumentParser()
        parser.add_argument("--debug", action="store_true", help="do more logging")
        parser.add_argument(
            "--dump", "-d", action="store_true", help="save files for testing the API"
        )
        parser.add_argument("--host", help="Elasticsearch server")
        parser.add_argument(
            "--limit", type=int, help="maximum concepts to fetch from the EVS"
        )
        parser.add_argument(
            "--sleep",
            type=float,
            metavar="SECONDS",
            help="longest delay between fetch failures",
        )
        parser.add_argument(
            "--port", type=int, help="Elasticsearch port (default 9200)"
        )
        parser.add_argument(
            "--test",
            "-t",
            action="store_true",
            help="save to file system, not Elasticsearch",
        )
        parser.add_argument(
            "--verbose",
            "-v",
            action="store_true",
            help="show progress on the command line",
        )
        parser.add_argument("--groups", help="dump file name for listing info records")
        parser.add_argument("--concepts", help="dump of concept values")
        parser.add_argument("--auth", "-a", help="comma-separated username/password")
        return parser.parse_args()

    @cached_property
    def overrides(self):
        """Hand-crafted labels and pretty URLs."""

        overrides = {}
        urls = {}
        with open(self.OVERRIDES, encoding="utf-8") as fp:
            for line in fp:
                override = Override(line)
                if override.url in urls:
                    message = f"URL {override.url} in multiple overrides"
                    raise ValueError(message)
                for code in override.codes:
                    if not code:
                        raise ValueError(f"empty code in {line}")
                    if code in overrides:
                        message = f"code {code} in multiple overrides"
                        raise ValueError(message)
                    overrides[code] = override
        return overrides

    @cached_property
    def port(self):
        """TCP/IP port on which we connect."""

        port = self.opts.port or self.PORT
        if self.verbose:
            stderr.write(f"connecting on port {port}\n")
        self.logger.info("connecting on port %s", port)
        return port

    @cached_property
    def stamp(self):
        """Date/time string used to create unique names."""
        return datetime.now().strftime("%Y%m%d%H%M%S")

    @cached_property
    def testing(self):
        """If True, write to file system instead of Elasticsearch."""
        return bool(self.opts.test)

    @cached_property
    def tokens(self):
        """Strings which we don't alter when we normalize display names."""

        tokens = set()
        with open(self.TOKENS, encoding="utf-8") as fp:
            for line in fp:
                tokens.add(line.strip())
        return tokens

    @cached_property
    def trial_def(self):
        """Schema for the listing trial records."""
        with open(self.TRIAL_DEF, encoding="utf-8") as fp:
            return load(fp)

    @cached_property
    def verbose(self):
        """Show progress (for running from the command line)."""
        return bool(self.opts.verbose)

    def __alert(self, e):
        """Send out email notification on failure.

        TODO:
            Implement method of sending notifications on failure.

        Pass:
            e - Exception caught during processing
        """

        stderr.write(f"failure: {e}\n")

    def __cleanup(self):
        """Prune old indices."""

        indices = self.es.cat.indices(params={"format": "json"})
        cutoff_date = date.today() - timedelta(self.DAYS_TO_KEEP)
        stamp = cutoff_date.strftime("%Y%m%d")
        for alias in (self.INFO_ALIAS, self.TRIAL_ALIAS):
            pattern = f"{alias}-20"
            cutoff = f"{alias}-{stamp}"
            self.logger.info("Cleanup cutoff: %s", cutoff)
            candidates = []
            for index in indices:
                name = index["index"]
                if name.startswith(pattern):
                    candidates.append(name)
            kept = min(len(candidates), self.INDICES_TO_KEEP)
            candidates = sorted(candidates)[: -self.INDICES_TO_KEEP]
            for name in candidates:
                if name < cutoff:
                    self.logger.info("dropping index %s", name)
                    self.es.indices.delete(name)
                else:
                    kept += 1
            if kept == 1:
                self.logger.info("Kept one %s index", alias)
            else:
                self.logger.info("Kept %d %s indices", kept, alias)

    def __create_alias(self, index, alias):
        """Point the canonical name for the records to our new index."""

        actions = []
        if self.es.indices.exists_alias(name=alias):
            aliases = self.es.indices.get_alias(index=alias)
            for old_index in aliases:
                if "aliases" in aliases[old_index]:
                    if alias in aliases[old_index]["aliases"]:
                        actions.append(
                            {
                                "remove": {
                                    "index": old_index,
                                    "alias": alias,
                                }
                            }
                        )
        actions.append({"add": {"index": index, "alias": alias}})
        self.logger.debug("actions: %s", actions)
        self.es.indices.update_aliases(body={"actions": actions})

    def __dump(self, groups, labels):
        """Create JSON Lines files for API testing.

        Pass:
          groups - sequence of concept groups for ListingInfo records
          labels - sequence of value dictionaries for TrialTypeInformation
        """

        action = {"index": {"_index": self.INFO_ALIAS}}
        action = dumps(action)
        with open(f"../dumps/{self.INFO}.jsonl", "w", encoding="utf-8") as fp:
            for group in groups:
                fp.write(f"{action}\n")
                fp.write(f"{dumps(group)}\n")
        action = {"index": {"_index": self.TRIAL_ALIAS}}
        action = dumps(action)
        with open(f"../dumps/{self.TRIAL}.jsonl", "w", encoding="utf-8") as fp:
            for label in labels:
                fp.write(f"{action}\n")
                fp.write(f"{dumps(label)}\n")

    def __fetch(self, codes, concept_dictionary):
        """Fetch concepts and their children from the EVS recursively.

        Populates the `concepts` property as a side effect.

        We attempt repeatedly to fetch the concepts until we succeed
        or run out of patience and conclude that the EVS is ailing.
        Note that we inject a tiny bit of sleep between each fetch
        even when there are no failures, to increase the chances that
        the EVS will be able to keep up. :-)

        Pass:
            codes - sequence of strings for the unique concept IDs in the EVS
            concept_dictionary - dictionary to be populated
        """

        seconds = self.MIN_SLEEP
        url = f"{self.API}?include=full&list={','.join(codes)}"
        while True:
            sleep(seconds)
            try:
                response = get(url, timeout=300)
                concepts = response.json()
                break
            except (RequestException, JSONDecodeError) as e:
                self.logger.exception(url)
                seconds *= 2
                if seconds > self.max_sleep:
                    self.logger.error("EVS has died -- bailing")
                    raise RuntimeError("EVS has died") from e
        if len(codes) != len(concepts):
            self.logger.warning("got %d concepts for %r", len(concepts), codes)
            self.logger.warning(response.text)
        for values in concepts:
            concept = Concept(values)
            concept_dictionary[concept.code] = concept
            if self.verbose:
                stderr.write(f"\rfetched {len(concept_dictionary)} concepts")
            self.logger.debug("fetched %s", concept.code)
        children = set()
        for values in concepts:
            if len(concept_dictionary) >= self.limit:
                break
            for child in values.get("children", []):
                if len(concept_dictionary) + len(children) >= self.limit:
                    break
                code = child.get("code", "").upper()
                if code and code not in concept_dictionary:
                    children.add(code)
        if children:
            i = 0
            children = list(children)
            while i < len(children):
                self.__fetch(children[i : i + self.BATCH_SIZE], concept_dictionary)
                i += self.BATCH_SIZE

    def __index(self, groups, labels):
        """Create Elasticsearch indexes, load them, and alias them.

        Pass:
          groups - sequence of concept groups for ListingInfo records
          labels - sequence of value dictionaries for TrialTypeInformation
        """

        start = datetime.now()
        info_index = f"{self.INFO_ALIAS}-{self.stamp}"
        trial_index = f"{self.TRIAL_ALIAS}-{self.stamp}"
        if self.verbose:
            stderr.write(f"creating {info_index}\n")
        self.es.indices.create(index=info_index, **self.info_def)
        if self.verbose:
            stderr.write(f"creating {trial_index}\n")
        self.es.indices.create(index=trial_index, **self.trial_def)
        if self.verbose:
            stderr.write("indexes created\n")
        actions = [{"_index": info_index, "_source": g} for g in groups]
        helpers.bulk(self.es, actions)
        if self.verbose:
            stderr.write(f"{info_index} populated\n")
        actions = [{"_index": trial_index, "_source": l} for l in labels]
        helpers.bulk(self.es, actions)
        if self.verbose:
            stderr.write(f"{trial_index} populated\n")
        params = {"max_num_segments": 1}
        self.es.indices.forcemerge(index=info_index, params=params)
        self.es.indices.forcemerge(index=trial_index, params=params)
        if self.verbose:
            stderr.write("indexes merged\n")
        if not self.opts.limit:
            self.__create_alias(info_index, self.INFO_ALIAS)
            self.__create_alias(trial_index, self.TRIAL_ALIAS)
            if self.verbose:
                stderr.write("index aliases updated\n")
            self.__cleanup()
            if self.verbose:
                stderr.write("old indices pruned\n")
        self.logger.info("indexing completed in %s", datetime.now() - start)

    def __load_groups_from_cache(self):
        """Broken out to keep pylint happy"""

        filename = self.opts.groups
        if not filename:
            return None
        path = self.USER_CWD / filename
        if not path.exists():
            path = Path(filename)
        if not path.exists():
            path = Path("../dumps") / filename
        if not path.exists():
            raise RuntimeError(f"{filename} not found")
        groups = []
        with path.open(encoding="utf-8") as fp:
            for line in fp:
                values = loads(line.strip())
                if "concept_id" in values:
                    groups.append(values)
        return groups


class Override:
    """Replacement values for a group of concepts."""

    BLANK = "<BLANK>"
    INHERIT = "<INHERIT>"
    URL_PATTERN = re_compile("^[a-z0-9-]+$")

    def __init__(self, line):
        """Parse the replacement record.

        Original version did all the work in the constructor. Broken out into
        property methods to make pylint happy.

        Pass:
          line - override values in the form CODE[,CODE[,...]]|LABEL|URL
        """

        self.__line = line.strip()
        self.matched_by = None

    @cached_property
    def __fields(self):
        """Components of the override definition"""
        fields = self.__line.split("|")
        if len(fields) != 3:
            raise ValueError(f"malformed override {self.__line}")
        return fields

    @cached_property
    def codes(self):
        """Codes for concepts using the override"""

        codes = {code.strip().upper() for code in self.__fields[0].split(",")}
        if not codes:
            raise ValueError(f"missing codes in {self.__line}")
        for code in codes:
            if not code:
                raise ValueError(f"empty code in {self.__line}")
        return codes

    @cached_property
    def label(self):
        """Label to use for the override"""

        label = self.__fields[1].strip()
        if not label:
            raise ValueError(f"empty label in {self.__line}")
        return label

    @cached_property
    def url(self):
        """URL to use for the override"""

        url = self.__fields[2].strip()
        if not url:
            raise ValueError(f"empty URL in {self.__line}")
        if not self.URL_PATTERN.match(url):
            if url not in (self.BLANK, self.INHERIT):
                raise ValueError(f"invalid override URL {url!r}")
        if len(url) > Loader.MAX_PRETTY_URL_LENGTH:
            raise ValueError(f"override URL {url} is too long")
        return url


class Concept:
    """Values for a single concept in the EVS."""

    SPACES = re_compile(r"\s+")
    NONE = "[NO DISPLAY NAME]"

    def __init__(self, values):
        """Pull out the pieces we need and discard the rest.

        Note:
            Original version did everything in the constructure, but pylint
            wasn't happy with that, so we've created a couple of property
            methods.

        Pass:
            values - dictionary of concept values pulled from EVS JSON
        """

        display_name = preferred_name = ctrp_name = None
        synonyms = values.get("synonyms", [])
        for synonym in synonyms:
            if synonym.get("source") == "CTRP":
                if synonym.get("termType") == "DN":
                    ctrp_name = (synonym.get("name") or "").strip()
                    if ctrp_name:
                        break
            else:
                name_type = synonym.get("type")
                if name_type == "Preferred_Name":
                    preferred_name = (synonym.get("name") or "").strip()
                elif name_type == "Display_Name":
                    display_name = (synonym.get("name") or "").strip()
        self.code = values["code"].upper()
        self.__name = ctrp_name or display_name or preferred_name or self.NONE

    @cached_property
    def name(self):
        """Normalized name string"""
        return self.SPACES.sub(" ", self.__name)

    @cached_property
    def key(self):
        """For dictionary lookup"""
        return self.name.lower()


class Group:
    """Set of concepts sharing a common normalized display string."""

    _FROM = "\u03b1\u03b2\u03bc;_&\u2013/"
    _TO = "abu-----"
    _STRIP = "\",+().\xaa'\u2019[\uff1a:*\\]"
    TRANS = str.maketrans(_FROM, _TO, _STRIP)
    NON_DIGITS = re_compile("[^0-9]+")

    def __init__(self, loader, concept):
        """Pull what we need from the caller's Concept object.

        Pass:
            loader - access to logger and tokens
            concept - object with the group's common display name
        """

        self.logger = loader.logger
        self.preserve = loader.tokens
        self.name = concept.name
        self.key = concept.key
        self.codes = []

    def __lt__(self, other):
        """Sort the groups by key."""
        return self.key < other.key

    @cached_property
    def phrase(self):
        """Display name mostly lowercased for use in running text.

        Lowercase each word in the name except for those appearing
        in self.preserve.
        """

        words = []
        for word in self.name.split():
            if word not in self.preserve:
                word = word.lower()
            words.append(word)
        return " ".join(words)

    @cached_property
    def values(self):
        """Dictionary of values to be serialized as JSON for the group."""

        codes = []
        for code in self.codes:
            codes.append(int(self.NON_DIGITS.sub("", code)))
        return {
            "concept_id": [f"C{code:d}" for code in sorted(codes)],
            "name": {
                "label": self.name,
                "normalized": self.phrase,
            },
            "pretty_url_name": self.url,
        }

    @cached_property
    def url(self):
        """Prepare the group's display name for use as a pretty URL."""

        name = self.key.replace(" ", "-").translate(self.TRANS)
        nfkd = normalize("NFKD", name)
        url = "".join([c for c in nfkd if not combining(c)])
        url = url.replace("%", "pct")
        if len(url) > Loader.MAX_PRETTY_URL_LENGTH:
            args = url, self.key
            self.logger.warning("dropping overlong url %r for %r", *args)
            return None
        return url


if __name__ == "__main__":
    Loader().run()
