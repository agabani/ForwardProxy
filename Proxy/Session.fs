module Session

open System
open System.Net
open System.Net.Sockets
open System.Text

open Http
open SocketExtensions

type Session()=
    static member Handle(socket:Socket) = async {
    
        try
            try
                use networkStream = new NetworkStream(socket)
                let header = Stream.GetHeader(networkStream)

                let content = System.Text.Encoding.UTF8.GetString header.Array
                let response = [|
                    "HTTP/1.1 200 OK"
                    "Content-Type: text/plain"
                    "Content-Length: " + content.Length.ToString()
                    ""
                    content
                |]

                let responseBytes = Encoding.ASCII.GetBytes(String.Join("\r\n", response))

                let! bytesSent = socket.AsyncSend(responseBytes)
                do! Async.Sleep 1000
            with
                e -> printfn "An error occurred: %s" e.Message
        finally
            socket.Shutdown SocketShutdown.Both
            socket.Close()
    }
