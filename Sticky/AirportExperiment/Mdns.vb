' mDNS example code, (c) 2012 Lucian Wischik
' ------------------------------
' This code send out a multicast UDP request for all local apple devices e.g. airport express
' mDNS is also known by names Zeroconf, Avahi and Bonjour (Apple's implementation)
'
' On my laptop+wifi, and my iPad+wifi, the multicast isn't working. Maybe the wifi
' station isn't forwarding multicast packets. So I wrote an alternative version
' which sends unicast probes to each device on the local subnet. The combination
' of both approaches ends up being more robust than itunes.
'
' Note: this code sends out requests for "RequestType=ANY", and so the answers it
' gets back are typically to big to fit into a packet. In that case this code will
' print just the fragment that did fit. Normally, you'd make a smaller request,
' and the results would all fit in a single packet, so there's no point making
' it stitch together multi-packet answers.

Module Mdns

    'Sub Main()
    '    Dim p As New Progress(Of String)(Sub(s) Console.WriteLine(s))
    '    TestMulticastFromConsoleAsync(p).Wait()
    '    TestUnicastFromConsoleAsync(p).Wait()
    'End Sub

    'Async Function TestUnicastFromConsoleAsync(progress As IProgress(Of String)) As Task
    '    For Each adapter In Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
    '        Dim ipaddr = adapter.GetIPProperties().UnicastAddresses.Where(Function(u) u.Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault()
    '        Dim gateway = adapter.GetIPProperties().GatewayAddresses.Where(Function(u) u.Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault()
    '        If ipaddr Is Nothing OrElse gateway Is Nothing Then Continue For
    '        Dim addr = ipaddr.Address.GetAddressBytes()
    '        Dim mask = ipaddr.IPv4Mask.GetAddressBytes()
    '        Console.WriteLine(adapter.Description)

    '        Using socket As New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Dgram, Net.Sockets.ProtocolType.Udp)
    '            socket.Bind(New Net.IPEndPoint(Net.IPAddress.Any, 0))
    '            Dim receiverTask = Task.Run(
    '                Sub()
    '                    Dim response = New Byte(51200) {}
    '                    Do
    '                        Try
    '                            socket.Receive(response)
    '                            If progress Is Nothing Then Continue Do
    '                            progress.Report(DnsResponseToString(response))
    '                        Catch ex As Net.Sockets.SocketException When ex.ErrorCode = 10054
    '                            ' disregard "ICMP connection refused" messages
    '                        Catch ex As Net.Sockets.SocketException When ex.ErrorCode = 10004
    '                            Return ' WSACancelBlockingCall, when the socket is closed
    '                        End Try
    '                    Loop
    '                End Sub)

    '            For Each targetip In IPRange(addr, mask)
    '                socket.SendTo(CreateDnsRequest(), New Net.IPEndPoint(New Net.IPAddress(targetip), 5353))
    '            Next

    '            Await Task.Delay(800)
    '            socket.Close()
    '            Await receiverTask
    '        End Using
    '    Next

    'End Function

    'Async Function TestMulticastFromConsoleAsync(progress As IProgress(Of String)) As Task
    '    Dim ipaddr = Net.IPAddress.Parse("224.0.0.251") ' this is standard multicast-address for mDNS
    '    Using socket As New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Dgram, Net.Sockets.ProtocolType.Udp)
    '        socket.SetSocketOption(Net.Sockets.SocketOptionLevel.Socket, Net.Sockets.SocketOptionName.ReuseAddress, True)
    '        socket.SetSocketOption(Net.Sockets.SocketOptionLevel.IP, Net.Sockets.SocketOptionName.AddMembership, New Net.Sockets.MulticastOption(ipaddr))
    '        socket.SetSocketOption(Net.Sockets.SocketOptionLevel.IP, Net.Sockets.SocketOptionName.MulticastTimeToLive, 5)

    '        socket.Bind(New Net.IPEndPoint(Net.IPAddress.Any, 5353))
    '        Dim receiverTask = Task.Run(
    '            Sub()
    '                Dim response = New Byte(51200) {}
    '                Do
    '                    Try
    '                        socket.Receive(response)
    '                        If progress Is Nothing Then Continue Do
    '                        progress.Report(DnsResponseToString(response))
    '                    Catch ex As Net.Sockets.SocketException When ex.ErrorCode = 10004
    '                        Return ' WSACancelBlockingCall, when the socket is closed
    '                    End Try
    '                Loop
    '            End Sub)

    '        socket.SendTo(CreateDnsRequest(), New Net.IPEndPoint(ipaddr, 5353))
    '        Await Task.Delay(800)
    '        socket.Close()
    '        Await receiverTask
    '    End Using
    'End Function


    ' Use this version for Win8 App Store
    Async Function TestUnicastFromUWPAsync(progress As IProgress(Of String)) As Task
        Dim reqbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer(CreateDnsRequest())

        For Each host In Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            If host.Type <> Windows.Networking.HostNameType.Ipv4 OrElse host.IPInformation Is Nothing OrElse host.IPInformation.NetworkAdapter Is Nothing Then Continue For
            Dim addr = host.RawName.Split("."c).Select(Function(s) Byte.Parse(s)).ToArray()
            If addr.Length > 0 AndAlso addr(0) = 169 Then Continue For
            Dim mask = BitConverter.GetBytes(CUInt(UInt32.MaxValue >> (32 - host.IPInformation.PrefixLength)))

            Using socket As New Windows.Networking.Sockets.DatagramSocket()
                AddHandler socket.MessageReceived,
                    Sub(sender, e)
                        Try
                            If progress Is Nothing Then Return
                            progress.Report(String.Format("RESPONSE from {0}:{1} to me here at {2}", e.RemoteAddress.DisplayName, e.RemotePort, e.LocalAddress.DisplayName))
                            Using reader = e.GetDataReader()
                                Dim response = New Byte(CInt(reader.UnconsumedBufferLength - 1)) {}
                                reader.ReadBytes(response)
                                progress.Report(DnsResponseToString(response))
                            End Using
                        Catch ex As Exception When ex.HResult = -2147014842
                            ' disregard "ICMP connection refused" messages
                        End Try
                    End Sub

                ' CAPABILITY: PrivateNetworks
                Await socket.BindEndpointAsync(Nothing, "")

                For Each targetip In IPRange(addr, mask)
                    Using Stream = Await socket.GetOutputStreamAsync(New Windows.Networking.HostName(String.Join(".", targetip)), "5353")
                        Await Stream.WriteAsync(reqbuf)
                    End Using
                Next targetip

                Await Task.Delay(800)
            End Using
        Next host
    End Function

    Async Function TestMulticastFromUWPAsync(progress As IProgress(Of String)) As Task
        Dim reqbuf = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsBuffer(CreateDnsRequest())
        Dim ipaddr = New Windows.Networking.HostName("224.0.0.251") ' standard multicast-address for mDNS

        Using socket As New Windows.Networking.Sockets.DatagramSocket()
            AddHandler socket.MessageReceived,
                    Sub(sender, e)
                        If progress Is Nothing Then Return
                        progress.Report(String.Format("RESPONSE from {0}:{1} to me here at {2}", e.RemoteAddress.DisplayName, e.RemotePort, e.LocalAddress.DisplayName))
                        Using reader = e.GetDataReader()
                            Dim response = New Byte(CInt(reader.UnconsumedBufferLength - 1)) {}
                            reader.ReadBytes(response)
                            progress.Report(DnsResponseToString(response))
                        End Using
                    End Sub

            ' CAPABILITY: PrivateNetworks
            Await socket.BindEndpointAsync(Nothing, "") '*** should be "5353" ???
            socket.Control.OutboundUnicastHopLimit = 5
            socket.JoinMulticastGroup(ipaddr) ' Alas there's no Win8 equivalent of ReuseAddress

            Using Stream = Await socket.GetOutputStreamAsync(ipaddr, "5353")
                Await Stream.WriteAsync(reqbuf)
            End Using

            Await Task.Delay(800)
        End Using
    End Function


    Function CreateDnsRequest() As Byte()
        Dim RequestID = 17 ' chosen arbitrarily by client (us!)
        Dim RequestType = 255 ' 1=ANAME/2=NS/6=SOA/15=MX/255=ANY/12=???
        Dim RequestClass = 255 ' 1=INTERNET/255=ANY
        '
        Return {
            CByte(RequestID >> 8), CByte(RequestID And 255),
            0, 0,
            0, 6,
            0, 0,
            0, 0,
            0, 0,
            13, AscW("_"), AscW("a"), AscW("p"), AscW("p"), AscW("l"), AscW("e"), AscW("t"), AscW("v"), AscW("-"), AscW("p"), AscW("a"), AscW("i"), AscW("r"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass),
            8, AscW("_"), AscW("a"), AscW("p"), AscW("p"), AscW("l"), AscW("e"), AscW("t"), AscW("v"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass),
            13, AscW("_"), AscW("t"), AscW("o"), AscW("u"), AscW("c"), AscW("h"), AscW("-"), AscW("r"), AscW("e"), AscW("m"), AscW("o"), AscW("t"), AscW("e"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass),
            5, AscW("_"), AscW("r"), AscW("a"), AscW("o"), AscW("p"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass),
            8, AscW("_"), AscW("a"), AscW("i"), AscW("r"), AscW("p"), AscW("o"), AscW("r"), AscW("t"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass),
            8, AscW("_"), AscW("a"), AscW("i"), AscW("r"), AscW("p"), AscW("l"), AscW("a"), AscW("y"), 4, AscW("_"), AscW("t"), AscW("c"), AscW("p"), 5, AscW("l"), AscW("o"), AscW("c"), AscW("a"), AscW("l"), 0, 0, CByte(RequestType), 0, CByte(RequestClass)
            }
    End Function


    Iterator Function IPRange(addrb As Byte(), maskb As Byte()) As IEnumerable(Of Byte())
        Dim addrl As UInt32 = CUInt(addrb(0)) << 24 Or CUInt(addrb(1)) << 16 Or CUInt(addrb(2)) << 8 Or addrb(3)
        Dim maskl As UInt32 = CUInt(maskb(0)) << 24 Or CUInt(maskb(1)) << 16 Or CUInt(maskb(2)) << 8 Or maskb(3)
        Dim wildl As UInt32 = Not maskl
        If addrl = 0 OrElse maskl = 0 Then Return
        Dim min = (addrl And maskl) + 1
        Dim max = (addrl Or wildl) - 1
        For i = min To max
            Dim a = {CByte(i >> 24 And 255), CByte(i >> 16 And 255), CByte(i >> 8 And 255), CByte(i And 255)}
            Debug.WriteLine(String.Join(".", a))
        Next
        For i = min To max
            Yield {CByte(i >> 24 And 255), CByte(i >> 16 And 255), CByte(i >> 8 And 255), CByte(i And 255)}
        Next
    End Function

    Function DnsResponseToString(response As Byte()) As String
        Dim s As New List(Of String)

        ' Parse the header of the response as per RFC 1035...
        Dim ResponseID As UShort = response(0) << 8 Or response(1)
        Dim IsResponse As Boolean = (response(2) And 128) <> 0
        Dim Opcode = (response(2) >> 3) And 15 ' 0=StandardQuery / 1=InverseQuery / 2=ServerStatusRequest
        Dim IsAuthoritative As Boolean = (response(2) And 4) <> 0
        Dim IsTruncated As Boolean = (response(2) And 2) <> 0
        Dim IsRecursionDesired As Boolean = (response(2) And 1) <> 0
        Dim IsRecursionAvailable = (response(3) And 128) <> 0
        Dim ResponseCode = (response(3) And 15) ' 0=OK / 1=FormatError / 2=ServerFailure / 3=NameError / 4=NotImplemented / 5=Refused
        Dim nQuestions = response(4) << 8 Or response(5)
        Dim nAnswers = response(6) << 8 Or response(7)
        Dim nNameServers = response(8) << 8 Or response(9)
        Dim nAdditionalRecords = response(10) << 8 Or response(11)
        Dim s1 = If(IsResponse, "RESPONSE#", "REQUEST#") & ResponseID.ToString() & " " & If(IsAuthoritative, "AA ", "")
        s1 &= If(IsTruncated, "TRUNC ", "") & If(IsRecursionDesired, "REC.DESIRED ", "") & If(IsRecursionAvailable, "REC.AVAILABLE ", "")
        s1 &= If(Opcode = 1, "INV.QUERY ", If(Opcode = 2, "STATUS.REQ", "")) & "RESPONSE.CODE=" & ResponseCode.ToString()
        s.Add(s1)

        ' Parse the Questions of the response
        s.Add("--- Questions ---")
        Dim off = 12, temp = 0
        For n = 0 To nQuestions - 1
            Dim Domain = ReadDomain(response, off, temp) : off += temp
            Dim RecordType = response(off) << 8 Or response(off + 1) : off += 2
            Dim DnsClass = response(off) << 8 Or response(off + 1) : off += 2
            s.Add(String.Format("QUESTION: {0}:{1} RR={2}", DnsClass, Domain, RecordType))
        Next

        ' Parse the answers and additional stuff...
        For n = 0 To nAnswers + nNameServers + nAdditionalRecords - 1
            If n = 0 Then : s.Add("--- Answers ---")
            ElseIf n = nNameServers Then : s.Add("--- NameServers ---")
            ElseIf n = nNameServers + nAdditionalRecords Then : s.Add("--- AdditionalRecords ---")
            End If

            Dim Domain = ReadDomain(response, off, temp) : off += temp
            Dim RecordType = response(off) << 8 Or response(off + 1) : off += 2
            Dim DnsClass = response(off) << 8 Or response(off + 1) : off += 2
            Dim TTL = response(off) << 24 Or response(off + 1) << 16 Or response(off + 2) << 8 Or response(off + 3) : off += 4
            Dim RecordLength = response(off) << 8 Or response(off + 1) : off += 2
            Dim roff = off : off += RecordLength

            Select Case RecordType
                Case 1 ' ANAME
                    Dim IP = {response(roff), response(roff + 1), response(roff + 2), response(roff + 3)} : roff += 4
                    s.Add(String.Format("ANAME: {0}", String.Join(".", IP)))
                Case 2 ' NS
                    Dim NS = ReadDomain(response, roff, temp) : roff += temp
                    s.Add(String.Format("NS: {0}", NS))
                Case 6 ' SOA
                    Dim PrimaryNameServer = ReadDomain(response, roff, temp) : roff += temp
                    Dim ResponsibleMailAddress = ReadDomain(response, roff, temp) : roff += temp
                    Dim Serial = response(roff) << 24 Or response(roff + 1) << 16 Or response(roff + 2) << 8 Or response(roff + 3) : roff += 4
                    Dim Refresh = response(roff) << 24 Or response(roff + 1) << 16 Or response(roff + 2) << 8 Or response(roff + 3) : roff += 4
                    Dim Retry = response(roff) << 24 Or response(roff + 1) << 16 Or response(roff + 2) << 8 Or response(roff + 3) : roff += 4
                    Dim Expire = response(roff) << 24 Or response(roff + 1) << 16 Or response(roff + 2) << 8 Or response(roff + 3) : roff += 4
                    Dim DefaultTTL = response(roff) << 24 Or response(roff + 1) << 16 Or response(roff + 2) << 8 Or response(roff + 3) : roff += 4
                    s.Add(String.Format("SOA: PNS={0}, RMA={1}, Serial={2}, Refresh={3}, Retry={4}, Expire={5}, DefTTL={6}", PrimaryNameServer, ResponsibleMailAddress, Serial, Refresh, Retry, Expire, DefaultTTL))
                Case 12 ' PTR
                    Dim PTR = ReadDomain(response, roff, temp) : roff += temp
                    s.Add(String.Format("PTR: {0}", PTR))
                Case 15 ' MX
                    Dim Preference = response(roff) << 8 Or response(roff + 1) : roff += 2
                    Dim MX = ReadDomain(response, roff, temp) : roff += temp
                    s.Add(String.Format("MX: #{0} -> {1}", Preference, MX))
                Case 16 ' TXT
                    Dim len = response(roff) : roff += 1
                    Dim TXT = System.Text.Encoding.UTF8.GetString(response, roff, len) : roff += len
                    s.Add(String.Format("TXT: {0}", TXT))
                Case 28 ' AAAA
                    Dim IP = New Byte(15) {} : Array.Copy(response, roff, IP, 0, 16) : roff += 16
                    s.Add(String.Format("AAAA: {0}", String.Join(":", IP)))
                Case 33 ' SRV
                    Dim Priority = response(roff) << 8 Or response(roff + 1) : roff += 2
                    Dim Weight = response(roff) << 8 Or response(roff + 1) : roff += 2
                    Dim Port = response(roff) << 8 Or response(roff + 1) : roff += 2
                    Dim Target = ReadDomain(response, roff, temp) : roff += temp
                    s.Add(String.Format("SRV: Priority={0} Weight={1} Port={2} Target={3}", Priority, Weight, Port, Target))
                Case Else
                    s.Add(String.Format("NOT-IMPLEMENTED RECORD TYPE {0}", RecordType))
            End Select
            s.Add(String.Format(" ({0}:{1} RR={2} TTL={3})", DnsClass, Domain, RecordType, TTL))
        Next

        Return String.Join(vbCrLf, s)
    End Function

    Function ReadDomain(buffer As Byte(), offset As Integer, <Runtime.InteropServices.Out> ByRef length As Integer) As String
        length = 0
        Dim r = ""
        Do
            Dim i As Byte = buffer(offset + length) : length += 1
            If i = 0 Then Return r
            If r.Length > 0 Then r += "."
            If (i And &HC0) <> &HC0 Then r += System.Text.Encoding.UTF8.GetString(buffer, offset + length, i) : length += i : Continue Do
            Dim ptr As Integer = (i And &H3F) << 8 Or buffer(offset + length) : length += 1
            Dim temp = 0 : Return r + ReadDomain(buffer, ptr, temp)
        Loop
    End Function


End Module

