module Tcp

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

open Session
open SocketExtensions

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
            Async.Start (Session.Handle socket, cancellationToken = cts.Token)
            return! loop()
        }

        Async.Start(loop(), cancellationToken = cts.Token)
        { new IDisposable with member x.Dispose() = cts.Cancel(); listener.Close() }
