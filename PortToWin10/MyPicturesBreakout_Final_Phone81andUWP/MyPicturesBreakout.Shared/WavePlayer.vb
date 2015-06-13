Imports SharpDX.IO
Imports SharpDX.Multimedia
Imports SharpDX.XAudio2
Imports Windows.Storage

Public Class WavePlayer
    Implements IDisposable

    Private ReadOnly xAudio As New XAudio2
    ReadOnly sounds As New Dictionary(Of String, BufferWithMetadata)
    ReadOnly voices As New Dictionary(Of BufferWithMetadata, Queue(Of SourceVoice))

    Public Sub Dispose() Implements IDisposable.Dispose
        xAudio.Dispose()
    End Sub

    Public Sub New()
        xAudio.StartEngine()
        Call New MasteringVoice(xAudio).SetVolume(1)
    End Sub

    Public Sub StartPlay(file As StorageFile)
        Task.Run(Sub() StartPlayInternal(file))
    End Sub

    Private Sub StartPlayInternal(file As StorageFile)
        Dim filepath = file.Path
        Dim wav As BufferWithMetadata, freelist As Queue(Of SourceVoice)

        ' CONCURRENCY...
        ' Synclock on "sounds" is for adding/retrieving a wav from "sounds", and for adding/retrieving
        ' the corresponding freelist.
        ' Synclock on "freelist" is for adding/removing elements from it

        SyncLock sounds
            If sounds.ContainsKey(filepath) Then
                wav = sounds(filepath)
                freelist = voices(wav)
            Else
                Using nfs As New NativeFileStream(filepath, NativeFileMode.Open, NativeFileAccess.Read), ss As New SoundStream(nfs)
                    Dim buf As New AudioBuffer With {.Stream = ss, .AudioBytes = CInt(ss.Length), .Flags = BufferFlags.EndOfStream}
                    wav = New BufferWithMetadata With {.Buffer = buf, .DecodedPacketsInfo = ss.DecodedPacketsInfo, .WaveFormat = ss.Format}
                End Using
                freelist = New Queue(Of SourceVoice)
                voices.Add(wav, freelist)
                sounds.Add(filepath, wav)
            End If
        End SyncLock

        Dim voice As SourceVoice
        SyncLock freelist
            If freelist.Count > 0 Then
                voice = freelist.Dequeue()
            Else
                voice = New SourceVoice(xAudio, wav.WaveFormat)
            End If
        End SyncLock

        Dim lambda As Action(Of IntPtr) = Sub(i)
                                              RemoveHandler voice.BufferEnd, lambda
                                              SyncLock freelist
                                                  freelist.Enqueue(voice)
                                              End SyncLock
                                          End Sub
        AddHandler voice.BufferEnd, lambda
        voice.SubmitSourceBuffer(wav.Buffer, wav.DecodedPacketsInfo)
        voice.Start()
    End Sub

    Public Class BufferWithMetadata
        Public Property Buffer As AudioBuffer
        Public Property DecodedPacketsInfo As UInteger()
        Public Property WaveFormat As WaveFormat
    End Class
End Class
