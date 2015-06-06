' RAOP example code, (c) 2012 Lucian Wischik
' -----------------------------------------------------------
' This code sends music to an Airport Express or compatible device.
' The Airport Express uses Apple's RAOP protocol, which is mostly like RTSP. Documentation below.
' For some reason this works in more cases than iTunes does. Not sure why.
' http://msdn.microsoft.com/en-us/library/ff361951(v=prot.10).aspx
' http://xmms2.org/wiki/Technical_note_that_describes_the_Remote_Audio_Access_Protocol_(RAOP)_used_in_AirTunes
' http://git.zx2c4.com/Airtunes2/about


Module Raop

    'Sub Main()
    '    Dim p As New Progress(Of String)(Sub(s) Console.WriteLine(s))
    '    TestFromConsoleAsync(p).Wait()
    'End Sub

    'Async Function TestFromConsoleAsync(progress As IProgress(Of String)) As Task
    '    ' We need the IP address of the airport. I've hard-coded it here. Should really discover it through mDNS.
    '    Dim airport_ip = "192.168.0.147"

    '    Dim alg As New System.Security.Cryptography.AesManaged() With {
    '        .Mode = System.Security.Cryptography.CipherMode.CBC,
    '        .Padding = System.Security.Cryptography.PaddingMode.None,
    '        .Key = New Byte() {157, 59, 155, 31, 73, 232, 20, 153, 141, 9, 251, 211, 207, 22, 190, 247},
    '        .IV = New Byte() {177, 173, 1, 199, 243, 191, 79, 70, 187, 233, 254, 190, 38, 178, 182, 58}}

    '    'If we wanted to generate a fresh AES key/iv each time ...
    '    ' uncomment this code, and use "aesiv/rasaeskey" instead of the hard-coded ones below
    '    'Dim aesiv As String
    '    'Dim rsaaeskey As String
    '    'alg.KeySize = 128
    '    'alg.GenerateKey()
    '    'alg.GenerateIV()
    '    'aesiv = Convert.ToBase64String(alg.IV).Replace("==", "")
    '    'Using rsa As New System.Security.Cryptography.RSACryptoServiceProvider()
    '    '    rsa.ImportParameters(New System.Security.Cryptography.RSAParameters() With {
    '    '        .Exponent = New Byte() {1, 0, 1},
    '    '        .Modulus = New Byte() {231, 215, 68, 242, 162, 226, 120, 139, 108, 31, 85, 160, 142, 183, 5, 68, 168,
    '    '                            250, 121, 69, 170, 139, 230, 198, 44, 229, 245, 28, 189, 212, 220, 104, 66,
    '    '                            254, 61, 16, 131, 221, 46, 222, 193, 191, 212, 37, 45, 192, 46, 111, 57, 139,
    '    '                            223, 14, 97, 72, 234, 132, 133, 94, 46, 68, 45, 166, 214, 38, 100, 246, 116,
    '    '                            161, 243, 4, 146, 154, 222, 79, 104, 147, 239, 45, 246, 231, 17, 168, 199,
    '    '                            122, 13, 145, 201, 217, 128, 130, 46, 80, 209, 41, 34, 175, 234, 64, 234, 159,
    '    '                            14, 20, 192, 247, 105, 56, 197, 243, 136, 47, 192, 50, 61, 217, 254, 85, 21,
    '    '                            95, 81, 187, 89, 33, 194, 1, 98, 159, 215, 51, 82, 213, 226, 239, 170, 191,
    '    '                            155, 160, 72, 215, 184, 19, 162, 182, 118, 127, 108, 60, 207, 30, 180, 206,
    '    '                            103, 61, 3, 123, 13, 46, 163, 12, 95, 255, 235, 6, 248, 208, 138, 221, 228,
    '    '                            9, 87, 26, 156, 104, 159, 239, 16, 114, 136, 85, 221, 140, 251, 154, 139, 239,
    '    '                            92, 137, 67, 239, 59, 95, 170, 21, 221, 230, 152, 190, 221, 243, 89, 150, 3,
    '    '                            235, 62, 111, 97, 55, 43, 182, 40, 246, 85, 159, 89, 154, 120, 191, 80, 6,
    '    '                            135, 170, 127, 73, 118, 192, 86, 45, 65, 41, 86, 248, 152, 158, 24, 166, 53,
    '    '                            91, 216, 21, 151, 130, 94, 15, 200, 117, 52, 62, 199, 130, 17, 118, 37, 205,
    '    '                            191, 152, 68, 123}})
    '    '    rsaaeskey = Convert.ToBase64String(rsa.Encrypt(alg.Key, True)).Replace("==", "")
    '    'End Using


    '    ' Start an RTSP dialog with the airport on port 5000 - control signals go over rtsp
    '    Dim rtsp As New Net.Sockets.TcpClient
    '    Await rtsp.ConnectAsync(airport_ip, 5000)
    '    Dim stream = rtsp.GetStream()
    '    Dim stdin As New IO.StreamReader(stream)
    '    Dim stdout As New IO.StreamWriter(stream)
    '    Dim msg = "", rsp = ""

    '    msg = "ANNOUNCE rtsp://localhost RTSP/1.0" & vbCrLf
    '    msg &= "CSeq: 1" & vbCrLf
    '    msg &= "Content-Type: application/sdp" & vbCrLf
    '    msg &= "Content-Length: 461" & vbCrLf ' Note that 461 is the hard-coded exact length of the "a=..." portion
    '    msg &= vbCrLf
    '    msg &= "a=rtpmap:96 AppleLossless" & vbCrLf
    '    msg &= "a=fmtp:96 4096 0 16 40 10 14 2 255 0 0 44100" & vbCrLf
    '    msg &= "a=rsaaeskey:KAFIPVmIQhverGlM/xy0q29g/iIzekxg9R+qWGvEcOq5/5uF7ptimJiwC6/q//Cq5r5u" &
    '           "FxSxPG2WcpSfICoihT01wWisteMC6Emyq6Z86z+m8iR4YdnoFKOoLpFLj11JS17gVUpnBVpBeU2GwwjX" &
    '           "UBmeux+ThHYNFRBUnwBPEqeFTEAXowbUmNNFhmh+JErwnnICD7PETgAzoYWu411duF5oba6A4HYM+giO" &
    '           "YuKxZhgZ6BmkUnn5CjbcPtMuPJXJ1qW8ks6dpNZSrqLkmdjj3/TQmiu4otfoLxigWa4BQBzydjo6ykFm" &
    '           "1rV1mL+J3Fi1A5VRBR8AxHv2mB4VLTTtqA" & vbCrLf
    '    msg &= "a=aesiv:sa0Bx/O/T0a76f6+JrK2Og" & vbCrLf
    '    rsp = Await TransmitAsync(stdout, stdin, msg, progress)

    '    msg = "SETUP rtsp://localhost RTSP/1.0" & vbCrLf
    '    msg &= "CSeq: 2" & vbCrLf
    '    msg &= "Transport: RTP/AVP/TCP;unicast;interleaved=0-1;mode=record;control_port=0;timing_port=0" & vbCrLf & vbCrLf
    '    rsp = Await TransmitAsync(stdout, stdin, msg, progress)
    '    Dim session = FindStr(rsp, "Session: ", vbCrLf)
    '    Dim port = CInt(FindStr(rsp, "server_port=", vbCrLf))

    '    msg = "RECORD rtsp://localhost RTSP/1.0" & vbCrLf
    '    msg &= "CSeq: 3" & vbCrLf
    '    msg &= "Session: " & session & vbCrLf
    '    msg &= "Range: npt=0-" & vbCrLf
    '    msg &= "RTP-Info: seq=0;rtptime=0" & vbCrLf & vbCrLf
    '    rsp = Await TransmitAsync(stdout, stdin, msg, progress)

    '    msg = "SET_PARAMETER rtsp://localhost RTSP/1.0" & vbCrLf
    '    msg &= "CSeq: 4" & vbCrLf
    '    msg &= "Session: " & session & vbCrLf
    '    msg &= "Content-Type: text/parameters" & vbCrLf
    '    msg &= "Content-Length: 13" & vbCrLf
    '    msg &= vbCrLf
    '    msg &= "volume: -50" & vbCrLf ' in range -140 to 0 or so. (make sure Content-Length field is correct)
    '    rsp = Await TransmitAsync(stdout, stdin, msg, progress)

    '    ' Start a RTP dialog with the airport, for the actual raw data
    '    Dim rtp As New Net.Sockets.TcpClient
    '    Await rtp.ConnectAsync(airport_ip, port)
    '    Dim stddata = rtp.GetStream()
    '    Dim pcm = New Byte(3 + 4096 * 4 - 1) {} ' holds pcm data (stereo, 16bit) for each 4096-sample transmission, with 23-bit header
    '    Dim enc = New Byte(4096 * 4 - 1) {} ' we encrpyt pcm into this bufer
    '    Dim packetheader = New Byte() {36, 0, 64, 15, 240, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

    '    ' We'd normally get data into pcm[3]...pcm[3+4096*4-1] by decoding a WMV, but I'll synthesize a sine wave instead.
    '    ' Each 4096-sample pcm, at 44100Hz, lasts about 1/10th second. So: hold each note for 10 pcms
    '    If Not progress Is Nothing Then progress.Report(vbCrLf & "=======>")
    '    Dim music = {247, 220, 196, 220, 247, 247, 247, 220, 220, 220, 247, 294, 294, 294}
    '    Dim phase = 0.0
    '    For imusic = 0 To (music.Length - 1) * 10
    '        Dim freq = If(imusic Mod 10 = 0, 0, music(imusic \ 10))

    '        For i = 0 To 4096 - 1
    '            phase += freq / 44100 * 2 * Math.PI
    '            Dim amplitude = If(freq = 0, 0, Math.Sin(phase) * 32767)
    '            Dim bb = BitConverter.GetBytes(CShort(amplitude)) ' bb[0] is LSB, bb[1] is MSB, of this twos-complement signed short
    '            pcm(3 + i * 4 + 0) = bb(1) ' left channel, MSB
    '            pcm(3 + i * 4 + 1) = bb(0) ' left channel, LSB
    '            pcm(3 + i * 4 + 2) = bb(1) ' right channel, MSB
    '            pcm(3 + i * 4 + 3) = bb(0) ' right channel, LSB
    '        Next

    '        ' First, add the 23-bit header
    '        pcm(0) = 32 ' bits 128+64+32: number of channels (2)
    '        pcm(1) = 0
    '        pcm(2) = 2 ' Bit 16: clear because we're using the default 4096-sample-size. Bit 2: IsUncompressed
    '        ' and because it's only 23-bits we have to shift everything else one bit to the left
    '        Dim b1 = pcm(3)
    '        pcm(2) = pcm(2) Or (b1 >> 7)
    '        For j = 3 To 3 + 4096 * 4 - 2
    '            Dim b0 = b1 : b1 = pcm(j + 1)
    '            pcm(j) = (b0 << 1) Or (b1 >> 7)
    '        Next
    '        pcm(3 + 4096 * 4 - 1) <<= 1

    '        ' Encrypt the buffer
    '        Using ms As New IO.MemoryStream(enc)
    '            Using cs As New System.Security.Cryptography.CryptoStream(ms, alg.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write)
    '                cs.Write(pcm, 0, 4096 * 4)
    '            End Using
    '        End Using

    '        ' Write out the packet
    '        If Not progress Is Nothing Then progress.Report(freq.ToString() & "Hz ")
    '        stddata.Write(packetheader, 0, packetheader.Length)
    '        stddata.Write(enc, 0, enc.Length)
    '        stddata.Write(pcm, 4096, 3) ' three bytes at the end that didn't get encrypted
    '    Next
    '    If Not progress Is Nothing Then progress.Report(vbCrLf)

    '    Await Task.Delay(9000)  ' sleep here to let the airport play through its buffer before we shut it down 

    '    msg = "TEARDOWN rtsp://localhost RTSP/1.0" & vbCrLf
    '    msg &= "CSeq: 4" & vbCrLf
    '    msg &= "Session: " & session & vbCrLf
    '    rsp = Await TransmitAsync(stdout, Nothing, msg, progress)

    '    stddata.Dispose()
    '    stdin.Dispose()
    '    stdout.Dispose()
    '    stream.Dispose()
    '    rtp.Close()
    '    rtsp.Close()
    '    alg.Dispose()
    'End Function



    Async Function TestFromUWPAsync(progress As IProgress(Of String)) As Task
        ' We need the IP address of the airport. I've hard-coded it here. Should really discover it through mDNS.
        Dim airport_ip = New Windows.Networking.HostName("192.168.0.120")
        progress.Report("Sending to hardcoded airport address 192.168.0.120")

        Dim keybuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer({157, 59, 155, 31, 73, 232, 20, 153, 141, 9, 251, 211, 207, 22, 190, 247})
        Dim ivbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer({177, 173, 1, 199, 243, 191, 79, 70, 187, 233, 254, 190, 38, 178, 182, 58})
        Dim alg = Windows.Security.Cryptography.Core.SymmetricKeyAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.SymmetricAlgorithmNames.AesCbc)
        Dim key = alg.CreateSymmetricKey(keybuf)

        ' Start an RTSP dialog with the airport on port 5000 - control signals go over rtsp
        Dim rtsp As New Windows.Networking.Sockets.StreamSocket
        Await rtsp.ConnectAsync(airport_ip, "5000")
        Dim stdin As New IO.StreamReader(rtsp.InputStream.AsStreamForRead())
        Dim stdout As New IO.StreamWriter(rtsp.OutputStream.AsStreamForWrite())
        Dim msg = "", rsp = ""

        msg = "ANNOUNCE rtsp://localhost RTSP/1.0" & vbCrLf
        msg &= "CSeq: 1" & vbCrLf
        msg &= "Content-Type: application/sdp" & vbCrLf
        msg &= "Content-Length: 461" & vbCrLf ' Note that 461 is the hard-coded exact length of the "a=..." portion
        msg &= vbCrLf
        msg &= "a=rtpmap:96 AppleLossless" & vbCrLf
        msg &= "a=fmtp:96 4096 0 16 40 10 14 2 255 0 0 44100" & vbCrLf
        msg &= "a=rsaaeskey:KAFIPVmIQhverGlM/xy0q29g/iIzekxg9R+qWGvEcOq5/5uF7ptimJiwC6/q//Cq5r5u" &
               "FxSxPG2WcpSfICoihT01wWisteMC6Emyq6Z86z+m8iR4YdnoFKOoLpFLj11JS17gVUpnBVpBeU2GwwjX" &
               "UBmeux+ThHYNFRBUnwBPEqeFTEAXowbUmNNFhmh+JErwnnICD7PETgAzoYWu411duF5oba6A4HYM+giO" &
               "YuKxZhgZ6BmkUnn5CjbcPtMuPJXJ1qW8ks6dpNZSrqLkmdjj3/TQmiu4otfoLxigWa4BQBzydjo6ykFm" &
               "1rV1mL+J3Fi1A5VRBR8AxHv2mB4VLTTtqA" & vbCrLf
        msg &= "a=aesiv:sa0Bx/O/T0a76f6+JrK2Og" & vbCrLf
        rsp = Await TransmitAsync(stdout, stdin, msg, progress)

        msg = "SETUP rtsp://localhost RTSP/1.0" & vbCrLf
        msg &= "CSeq: 2" & vbCrLf
        msg &= "Transport: RTP/AVP/TCP;unicast;interleaved=0-1;mode=record;control_port=0;timing_port=0" & vbCrLf & vbCrLf
        rsp = Await TransmitAsync(stdout, stdin, msg, progress)
        Dim session = FindStr(rsp, "Session: ", vbCrLf)
        Dim port = CInt(FindStr(rsp, "server_port=", vbCrLf))

        msg = "RECORD rtsp://localhost RTSP/1.0" & vbCrLf
        msg &= "CSeq: 3" & vbCrLf
        msg &= "Session: " & session & vbCrLf
        msg &= "Range: npt=0-" & vbCrLf
        msg &= "RTP-Info: seq=0;rtptime=0" & vbCrLf & vbCrLf
        rsp = Await TransmitAsync(stdout, stdin, msg, progress)

        msg = "SET_PARAMETER rtsp://localhost RTSP/1.0" & vbCrLf
        msg &= "CSeq: 4" & vbCrLf
        msg &= "Session: " & session & vbCrLf
        msg &= "Content-Type: text/parameters" & vbCrLf
        msg &= "Content-Length: 13" & vbCrLf
        msg &= vbCrLf
        msg &= "volume: -50" & vbCrLf ' in range -140 to 0 or so. (make sure Content-Length field is correct)
        rsp = Await TransmitAsync(stdout, stdin, msg, progress)

        ' Start a RTP dialog with the airport, for the actual raw data
        Dim rtp As New Windows.Networking.Sockets.StreamSocket
        Await rtp.ConnectAsync(airport_ip, CStr(port))
        Dim stddata = rtp.OutputStream
        Dim pcm = New Byte(3 + 4096 * 4 - 1) {} ' holds pcm data (stereo, 16bit) for each 4096-sample transmission, with 23-bit header
        Dim packetheader = New Byte() {36, 0, 64, 15, 240, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim packetheaderbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer(packetheader)


        ' We'd normally get data into pcm[3]...pcm[3+4096*4-1] by decoding a WMV, but I'll synthesize a sine wave instead.
        ' Each 4096-sample pcm, at 44100Hz, lasts about 1/10th second. So: hold each note for 10 pcms
        If Not progress Is Nothing Then progress.Report(vbCrLf & "=======>")
        Dim music = {247, 220, 196, 220, 247, 247, 247, 220, 220, 220, 247, 294, 294, 294}
        Dim phase = 0.0
        For imusic = 0 To (music.Length - 1) * 10
            Dim freq = If(imusic Mod 10 = 0, 0, music(imusic \ 10))

            For i = 0 To 4096 - 1
                phase += freq / 44100 * 2 * Math.PI
                Dim amplitude = If(freq = 0, 0, Math.Sin(phase) * 32767)
                Dim bb = BitConverter.GetBytes(CShort(amplitude)) ' bb[0] is LSB, bb[1] is MSB, of this twos-complement signed short
                pcm(3 + i * 4 + 0) = bb(1) ' left channel, MSB
                pcm(3 + i * 4 + 1) = bb(0) ' left channel, LSB
                pcm(3 + i * 4 + 2) = bb(1) ' right channel, MSB
                pcm(3 + i * 4 + 3) = bb(0) ' right channel, LSB
            Next

            ' First, add the 23-bit header
            pcm(0) = 32 ' bits 128+64+32: number of channels (2)
            pcm(1) = 0
            pcm(2) = 2 ' Bit 16: clear because we're using the default 4096-sample-size. Bit 2: IsUncompressed
            ' and because it's only 23-bits we have to shift everything else one bit to the left
            Dim b1 = pcm(3)
            pcm(2) = pcm(2) Or (b1 >> 7)
            For j = 3 To 3 + 4096 * 4 - 2
                Dim b0 = b1 : b1 = pcm(j + 1)
                pcm(j) = (b0 << 1) Or (b1 >> 7)
            Next
            pcm(3 + 4096 * 4 - 1) <<= 1

            ' Encrypt the buffer (and make a buffer for the 3 unencrypted bytes at the end)
            Dim pcmbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer(pcm, 0, 4096 * 4)
            Dim encbuf = Windows.Security.Cryptography.Core.CryptographicEngine.Encrypt(key, pcmbuf, ivbuf)
            Dim trailer = {pcm(4096), pcm(4097), pcm(4098)}
            Dim trailerbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer(trailer, 0, 3)

            ' Write out the packet
            If Not progress Is Nothing Then progress.Report(freq.ToString() & "Hz ")
            Await stddata.WriteAsync(packetheaderbuf)
            Await stddata.WriteAsync(encbuf)
            Await stddata.WriteAsync(trailerbuf)
        Next
        If Not progress Is Nothing Then progress.Report(vbCrLf)

        Await task.Delay(9000)  ' sleep here to let the airport play through its buffer before we shut it down

        msg = "TEARDOWN rtsp://localhost RTSP/1.0" & vbCrLf
        msg &= "CSeq: 4" & vbCrLf
        msg &= "Session: " & session & vbCrLf
        rsp = Await TransmitAsync(stdout, Nothing, msg, progress)

        stdin.Dispose()
        stdout.Dispose()
        rtp.Dispose()
        rtsp.Dispose()
    End Function


    Async Function TransmitAsync(stdout As IO.StreamWriter, stdin As IO.StreamReader, msg As String, progress As IProgress(Of String)) As Task(Of String)
        If Not progress Is Nothing Then progress.Report(vbCrLf & "------>" & vbCrLf & msg)
        Await stdout.WriteAsync(msg)
        Await stdout.FlushAsync()
        If stdin Is Nothing Then Return Nothing
        If Not progress Is Nothing Then progress.Report(New String(" "c, 30) & "<-------")
        msg = ""
        Do
            Dim line = Await stdin.ReadLineAsync()
            If String.IsNullOrEmpty(line) Then Exit Do
            If Not progress Is Nothing Then progress.Report(New String(" "c, 30) & line)
            msg &= line & vbCrLf
        Loop
        Return msg
    End Function

    Function FindStr(s As String, prefix As String, postfix As String) As String
        Dim i = s.IndexOf(prefix) : If i = -1 Then Return Nothing
        s = s.Substring(i + prefix.Length)
        Dim j = s.IndexOf(postfix) : If j = -1 Then Return Nothing
        s = s.Substring(0, j)
        Return s
    End Function

End Module


'' RAOP example code, (c) 2012 Lucian Wischik
'' -----------------------------------------------------------
'' This code sends music to an Airport Express or compatible device.
'' The Airport Express uses Apple's RAOP protocol, which is mostly like RTSP. Documentation below.
'' For some reason this works in more cases than iTunes does. Not sure why.
'' http://msdn.microsoft.com/en-us/library/ff361951(v=prot.10).aspx
'' http://xmms2.org/wiki/Technical_note_that_describes_the_Remote_Audio_Access_Protocol_(RAOP)_used_in_AirTunes
'' http://git.zx2c4.com/Airtunes2/about


