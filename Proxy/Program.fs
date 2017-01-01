open System.Threading

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let disposable = Server.Server.Start(port = 8889)
    Thread.Sleep(60 * 1000)
    disposable.Dispose()

    0
