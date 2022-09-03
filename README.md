# BDM Tool

MapAeg (Buondelmonti) import tool. See this project's [documentation](docs/index.md) for more.

This is work in progress.

## Quick Start

(1) get the DOCX document in some folder. Say it is `C:\users\dfusi\desktop\bdm\corfu.docx`.

(2) pick text and essential formatting:

```ps1
.\PickDocx pick C:\users\dfusi\Desktop\bdm\corfu.docx C:\users\dfusi\Desktop\bdm\ -f -x -m
```

This produces `corfu.xml` and `corfu_fmt.xml`.

(3) run a [Proteus pipeline](bdm-dump.json) to dump the parsing process:

```ps1
.\bdmtool import C:\users\dfusi\Desktop\bdm\bdm.json c:\users\dfusi\Desktop\bdm\
```

(4) create the MapAeg databases by starting its API. Then, delete parts and items if they were seeded.

(5) run a modified version of the [same pipeline](bdm-cadmus.json) by replacing the dump entry region parser with the true importer, and specifying a more specialized context, like this:

```json
  "Context": {
    "Id": "entry-set-context.cadmus"
  },
  "EntryRegionParsers": [
    {
      "Id": "entry-region-parser.bdm-text",
      "Options": {
        "GroupId": "corfu"
      }
    }
  ]
```

(6) you can now open the MapAeg app and browse the items.
