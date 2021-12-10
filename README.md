# convertFixings

## Command line usage

    convertFixings /csv:OUTPUT_FILES/REPORT.csv
                   /prod:INPUT_FILES/PRODUCT.xml
                   /vars:INPUT_FILES/VARIANTS.xml
                   /out:OUTPUT_FILES/EMS_IMPORT_FILE.xml
                   /csvxsl:P:/path/csv_transformer.xsl
                   /emsxsl:P:/path/ems_transformer.xsl
                   /prefix:X123_22_

### To show usage:

    convertFixings /h

### Options:

    --csv=PATH             PATH to source interim file
    --prod=PATH            PATH to Product Structure Export XML file
    --vars=PATH            PATH to eMS VariantSetLibrary Export XML file
    --prefix[=VARIABLE]    ExternalID prefix VARIABLE
    --out=PATH             PATH to eBop Import file
    --csvxsl=PATH          PATH to csv_transformer.xsl
    --emsxsl=PATH          PATH to ems_transformer.xsl
    -h, -?, --help         Shows this help message

## Attributions:
This work is a rewrite of the code originally written by Jerry Gadd in 18/01/2010
