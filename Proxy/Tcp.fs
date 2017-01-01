module Tcp

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

type Socket with
    member socket.AsyncAccept() =
        Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)
    member socket.AsyncReceive(buffer:byte[], ?offset, ?count) =
        let offset = defaultArg offset 0
        let count = defaultArg count buffer.Length
        let beginRecieve(b, o, c, cb, s) = socket.BeginReceive(b, o, c, SocketFlags.None, cb, s)
        Async.FromBeginEnd(buffer, offset, count, beginRecieve, socket.EndReceive)
    member socket.AsyncSend(buffer:byte[], ?offset, ?count) =
        let offset = defaultArg offset 0
        let count = defaultArg count buffer.Length
        let beginSend(b, o, c, cb, s) = socket.BeginSend(b, o, c, SocketFlags.None, cb, s)
        Async.FromBeginEnd(buffer, offset, count, beginSend, socket.EndSend)

type Server()=
    static member Start(?ipAddress, ?port) =
        let ipAddress = defaultArg ipAddress IPAddress.Any
        let port = defaultArg port 8999
        let endpoint = IPEndPoint(ipAddress, port)
        let cts = new CancellationTokenSource()
        let listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        listener.Bind endpoint
        listener.Listen(int SocketOptionName.MaxConnections)

        let rec loop() = async {
            let! socket = listener.AsyncAccept()
            let response = [|
                "HTTP/1.1 200 OK\r\n"B
                "Content-Type: text/plain\r\n"B
                "Content-Length: 12\r\n"B 
                "\r\n"B
                "Hello World!"B|] |> Array.concat

            try
                try
                    let! bytesSent = socket.AsyncSend(response)
                    printfn "bytes sent %d" bytesSent
                    ()
                with
                    e -> printfn "An error occurred: %s" e.Message
            finally
                socket.Shutdown(SocketShutdown.Both)
                socket.Close()
                
            return! loop()
        }

        Async.Start(loop(), cancellationToken = cts.Token)
        { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Close() }
