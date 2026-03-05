module internal BepuFSharp.ContactEvents

open System.Collections.Generic

[<AllowNullLiteral>]
type ContactEventBuffer() =
    let writeBuffer = ResizeArray<ContactEvent>()
    let mutable readBuffer = Array.empty<ContactEvent>
    let previousPairs = HashSet<uint64>()
    let currentPairs = HashSet<uint64>()
    let lockObj = obj()

    static member PairKey(a: int, b: int) : uint64 =
        let lo = min a b
        let hi = max a b
        (uint64 (uint32 lo)) ||| (uint64 (uint32 hi) <<< 32)

    member _.AppendContact(event: ContactEvent) =
        lock lockObj (fun () -> writeBuffer.Add(event))

    member _.TrackPair(a: int, b: int) =
        lock lockObj (fun () -> currentPairs.Add(ContactEventBuffer.PairKey(a, b)) |> ignore)

    member _.SwapBuffers() =
        lock lockObj (fun () ->
            // Emit Ended events for pairs in previous but not current
            for pair in previousPairs do
                if not (currentPairs.Contains(pair)) then
                    let lo = int (uint32 (pair &&& 0xFFFFFFFFuL))
                    let hi = int (uint32 (pair >>> 32))
                    writeBuffer.Add({
                        BodyA = if lo < 0x40000000 then ValueSome(BodyId lo) else ValueNone
                        StaticA = if lo >= 0x40000000 then ValueSome(StaticId(lo &&& 0x3FFFFFFF)) else ValueNone
                        BodyB = if hi < 0x40000000 then ValueSome(BodyId hi) else ValueNone
                        StaticB = if hi >= 0x40000000 then ValueSome(StaticId(hi &&& 0x3FFFFFFF)) else ValueNone
                        Normal = System.Numerics.Vector3.Zero
                        Depth = 0.0f
                        EventType = Ended
                    })
            readBuffer <- writeBuffer.ToArray()
            writeBuffer.Clear()
            previousPairs.Clear()
            for pair in currentPairs do
                previousPairs.Add(pair) |> ignore
            currentPairs.Clear()
        )

    member _.GetEvents() : ContactEvent[] = readBuffer
