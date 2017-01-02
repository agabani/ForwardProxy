module Http

open System
open System.IO
open System.Linq
open System.Net.Sockets
open System.Text

type Address = { Hostname : string; Port : int }

type Header(array)=
    static let ContentLengthKey = "Content-Length:"
    static let HostKey = "Host:"

    member this.Array           : byte[]    = array
    member this.List            : string[]  = Encoding.ASCII.GetString(array).Split([|"\r\n"|], StringSplitOptions.RemoveEmptyEntries)
    member this.Verb            : string    = this.List.First().Split(' ').First()
    member this.ContentLength   : int64     = 
        let header = this.List.SingleOrDefault(fun string -> string.StartsWith(ContentLengthKey, StringComparison.OrdinalIgnoreCase))
        match header with
        | null -> 0L
        | _ -> Int64.Parse(header.Substring(ContentLengthKey.Length).TrimStart())
    member this.Host            : Address   =
        let header = this.List.Single(fun string -> string.StartsWith(HostKey, StringComparison.OrdinalIgnoreCase)).Substring(HostKey.Length).TrimStart().Split(':')
        match header.Length with
        | 1 -> {Hostname = header.First(); Port = 80 }
        | 2 -> {Hostname = header.First(); Port = Int32.Parse(header.Last())}
        | _ -> raise (System.Exception("Invalid address."))

type Stream()=
    static let Delimiter = [|'\r'B; '\n'B; '\r'B; '\n'B|]

    static member private ReadStream(networkStream:NetworkStream) =
        let memoryStream = new MemoryStream()
        let rec loop = fun (input:NetworkStream) (output:MemoryStream) counter ->
            let readInt = networkStream.ReadByte()
            match readInt with
            | -1 -> raise (System.Exception("No bytes available."))
            | _ ->
                let readByte : byte = byte readInt
                output.WriteByte readByte
                match (Delimiter.[counter] = readByte, counter) with
                | (false, _ ) -> loop input output 0
                | (true,  3 ) -> ()
                | (true,  _ ) -> loop input output (counter + 1)
        loop networkStream memoryStream 0
        memoryStream

    static member GetHeader(networkStream:NetworkStream)=
        use memoryStream = Stream.ReadStream (networkStream)
        new Header(memoryStream.ToArray())
