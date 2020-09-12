// Learn more about F# at http://fsharp.org

open GratisDns.Commands

[<EntryPoint>]
let main argv =
    let app = new RootCommand()
    app.Execute(argv) |> ignore
    1