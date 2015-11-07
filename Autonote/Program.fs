// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open EvernoteSDK
open EvernoteSDK.Advanced
open System
open System.Collections.Generic
open System.IO
open System.Reactive
open System.Reactive.Linq
open System.Text.RegularExpressions
open SharpConfig

module FileSystem = 
    type FileSystemEvent = 
        | Created of file : FileInfo
        | Changed of file : FileInfo
        | Deleted of file : FileInfo
        | Renamed of file : FileInfo * oldFile : FileInfo
    
    let fromFileSystemWatcher directory = 
        Observable.Create<FileSystemEvent>(fun (observer : IObserver<FileSystemEvent>) -> 
            let fsw = new FileSystemWatcher(directory)
            fsw.Created.AddHandler(fun _ args -> observer.OnNext(Created(FileInfo(args.FullPath))))
            fsw.Changed.AddHandler(fun _ args -> observer.OnNext(Changed(FileInfo(args.FullPath))))
            fsw.Deleted.AddHandler(fun _ args -> observer.OnNext(Deleted(FileInfo(args.FullPath))))
            fsw.Renamed.AddHandler(fun _ args -> observer.OnNext(Renamed(FileInfo(args.FullPath), FileInfo(args.OldFullPath))))
            fsw.EnableRaisingEvents <- true
            fsw :> IDisposable)

module Observable = 
    let delay (time : TimeSpan) stream = Observable.Delay(stream, time)

open FileSystem

[<EntryPoint>]
let main argv = 
    let cfg = Config("autonote", true)

    let developerToken = cfg.["developerToken"] :?> string
    let noteStoreUrl = cfg.["noteStoreUrl"] :?> string
    let watchPath = cfg.["watchPath"] :?> string
    let filedPath = cfg.["filedPath"] :?> string

    ENSessionAdvanced.SetSharedSessionDeveloperToken(developerToken, noteStoreUrl)
    let fileSystemEvents = fromFileSystemWatcher watchPath
    
    let parseFilename fileName = 
        let regex = Regex "([0-9]{4})-([0-9]{2})-([0-9]{2})(.*)\.pdf"
        let regexMatch = regex.Match fileName
        let year = Int32.Parse(regexMatch.Groups.[1].Value)
        let month = Int32.Parse(regexMatch.Groups.[2].Value)
        let day = Int32.Parse(regexMatch.Groups.[3].Value)
        let title = regexMatch.Groups.[4].Value.Trim()
        title, DateTime(year, month, day)
    
    let uploadFileToEvernote (file : FileInfo) = 
        let (noteName, createdDate) = parseFilename file.FullName
        let note = ENNote(Title = noteName, Content = ENNoteContent.NoteContentWithString(""))
        let fileContents = File.ReadAllBytes file.FullName
        note.Resources.Add(ENResource(fileContents, "application/pdf", file.Name))
        let uploadedNoteRef = ENSessionAdvanced.SharedSession.UploadNote(note, null)
        let noteStore = ENSessionAdvanced.SharedSession.NoteStoreForNoteRef uploadedNoteRef
        let edamNote = noteStore.GetNote(uploadedNoteRef.Guid, false, false, false, false)
        edamNote.Created <- createdDate.ToEdamTimestamp()
        noteStore.UpdateNote edamNote |> ignore
        printfn "Created note from %A" file.FullName
        File.Move(file.FullName, Path.Combine(filedPath, file.Name))
    
    use creationEventsHandler = 
        fileSystemEvents
        |> Observable.map (fun x -> 
               match x with
               | Created(file) -> Some(file)
               | _ -> None)
        |> Observable.filter Option.isSome
        |> Observable.map Option.get
        |> Observable.filter (fun file -> file.Extension.ToLower() = ".pdf")
        |> Observable.delay (TimeSpan.FromSeconds 10.0)
        |> Observable.subscribe uploadFileToEvernote
    
    Console.ReadLine() |> ignore
    printfn "%A" argv
    0 // return an integer exit code
