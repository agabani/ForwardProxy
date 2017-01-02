module Http

open System
open System.IO
open System.Linq
open System.Net.Sockets
open System.Text

type Header(array)=
    member this.Array = array
    member this.List  = Encoding.ASCII.GetString(array).Split([|"\r\n"|], StringSplitOptions.RemoveEmptyEntries)
    member this.Verb  = this.List.First().Split(' ').First()

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
