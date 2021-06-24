Imports System.IO
Module Module1
    Sub BlockRead(ByRef BR As BinaryReader, ByVal N As Integer, ByRef S As String)
        Dim c As Char
        Dim i As Integer

        S = ""
        For i = 1 To N
            c = Chr(BR.ReadByte)
            S = S + c
        Next
    End Sub

    Sub BlankRead(ByRef BR As BinaryReader, ByVal N As Integer)
        Dim i As Integer

        For i = 1 To N
            BR.ReadByte()
        Next
    End Sub

    Sub BlockReadInt(ByRef br As BinaryReader, ByVal N As Integer, ByRef L As Long)
        Dim i As Integer
        Dim m As Long
        Dim j As Byte

        L = 0
        m = 1
        For i = 1 To N
            j = br.ReadByte
            L = L + j * m
            m = m * 256
        Next
    End Sub

    Sub BlockWriteInt(ByRef bw As BinaryWriter, ByVal N As Integer, ByRef L As Long)
        Dim i As Integer
        Dim j As Byte

        For i = 1 To N
            j = L Mod 256
            bw.Write(CByte(j))
            L = L \ 256
        Next
    End Sub

    Function HexToDecLSBFirst(ByVal s As String) As Long
        Dim i As Integer
        Dim m, n As Long

        n = 0
        m = 1

        For i = 0 To s.Length - 1
            n = n + Asc(s(i)) * m
            m = m * 256
        Next

        Return n
    End Function
    Function opAND(ByVal C1 As System.Drawing.Color, ByVal c2 As System.Drawing.Color) As System.Drawing.Color
        Dim c As System.Drawing.Color
        Dim r, g, b As Byte

        r = C1.R And c2.R
        g = C1.G And c2.G
        b = C1.B And c2.B

        c = Color.FromArgb(r, g, b)

        Return c
    End Function

    Function opOR(ByVal C1 As System.Drawing.Color, ByVal c2 As System.Drawing.Color) As System.Drawing.Color
        Dim c As System.Drawing.Color
        Dim r, g, b As Byte

        r = C1.R Or c2.R
        g = C1.G Or c2.G
        b = C1.B Or c2.B

        c = Color.FromArgb(r, g, b)

        Return c
    End Function
End Module
