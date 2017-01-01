open System.Threading

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let disposable = Tcp.Server.Start(port = 8889)
    Thread.Sleep(120 * 1000)
    disposable.Dispose()

    0
