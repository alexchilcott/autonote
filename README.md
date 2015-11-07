# Autonote
Watches a particular directory for pdf files to be added to it, and uploads them to Evernote

# Usage
1. Compile by running build.bat or build.sh
2. Obtain an Evernote developer token at [from here](https://www.evernote.com/api/DeveloperToken.action)
3. Create an "autonote.json" file in the same path as "Autonote.exe" as so:

    {
        "developerToken": "<YOUR EVERNOTE DEVELOPER TOKEN>",
        "noteStoreUrl": "<YOUR EVERNOTE NOTE STORE URL>",
        "watchPath": "<THE PATH TO WATCH FOR PDFS>",
        "filedPath": "<THE PATH TO MOVE PROCESSED PDFS TO>"
    }
