module Session

open System.Net
open System.Net.Sockets

open SocketExtensions

type Session()=
    static member Handle(socket:Socket) = async {
    
        try
            try
                let response = [|
                    "HTTP/1.1 200 OK\r\n"B
                    "Content-Type: text/plain\r\n"B
                    "Content-Length: 12\r\n"B 
                    "\r\n"B
                    "Hello World!"B|] |> Array.concat

                let! bytesSent = socket.AsyncSend response
                ()
                do! Async.Sleep 1000
            with
                e -> printfn "An error occurred: %s" e.Message
        finally
            socket.Shutdown SocketShutdown.Both
            socket.Close()
    }
