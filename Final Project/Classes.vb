Imports System.IO

Public Enum StateBlastHornet
    intro
    idle
    hit
    stingAttack
    hitWall
    introBomb
    bomb
    afterBomb
    shield
    afterShield
    death
End Enum

Public Enum StateParasiteProjectile
    intro
End Enum

Public Enum StateCrossHairProjectile
    find
    transit
    lock
End Enum

Public Enum StateSpongeBob
    intro
    walk
    bubble
    death
End Enum

Public Enum StateBubbleProjectile
    attack
End Enum

Public Enum FaceDir
    Left
    Right
End Enum

Public Class CImage
    Public Width As Integer
    Public Height As Integer
    Public Elmt(,) As System.Drawing.Color
    Public ColorMode As Integer

    Sub OpenImage(ByVal FName As String)
        Dim s As String
        Dim L As Long
        Dim BR As BinaryReader
        Dim h, w, pos As Integer
        Dim r, g, b As Integer
        Dim pad As Integer

        BR = New BinaryReader(File.Open(FName, FileMode.Open))

        Try
            BlockRead(BR, 2, s)
            If s <> "BM" Then
                MsgBox("Not a BMP file")
            Else
                BlockReadInt(BR, 4, L)          'size
                BlankRead(BR, 4)                'reversed
                BlockReadInt(BR, 4, pos)        'start of data
                BlankRead(BR, 4)                'size of header
                BlockReadInt(BR, 4, Width)      'width
                BlockReadInt(BR, 4, Height)     'height
                BlankRead(BR, 2)                'color panels
                BlockReadInt(BR, 2, ColorMode)  'color mode
                If ColorMode <> 24 Then
                    MsgBox("It is not a 24-bit color BMP, please try again")
                Else
                    BlankRead(BR, pos - 30)
                    ReDim Elmt(Width - 1, Height - 1)
                    pad = (4 - (Width * 3 Mod 4)) Mod 4

                    For h = Height - 1 To 0 Step -1
                        For w = 0 To Width - 1
                            BlockReadInt(BR, 1, b)
                            BlockReadInt(BR, 1, g)
                            BlockReadInt(BR, 1, r)
                            Elmt(w, h) = Color.FromArgb(r, g, b)
                        Next
                        BlankRead(BR, pad)
                    Next
                End If
            End If
        Catch ex As Exception
            MsgBox("Error")
        End Try
        BR.Close()
    End Sub

    Sub CreateMask(ByRef Mask As CImage)
        Dim i, j As Integer
        Mask = New CImage
        Mask.Width = Width
        Mask.Height = Height

        ReDim Mask.Elmt(Mask.Width - 1, Mask.Height - 1)
        For i = 0 To Width - 1
            For j = 0 To Height - 1
                If Elmt(i, j).R = 0 And Elmt(i, j).G = 0 And Elmt(i, j).B = 0 Then
                    Mask.Elmt(i, j) = Color.FromArgb(255, 255, 255)
                Else
                    Mask.Elmt(i, j) = Color.FromArgb(0, 0, 0)
                End If
            Next
        Next
    End Sub

    Sub CopyImg(ByRef Img As CImage)
        Img = New CImage
        Img.Width = Width
        Img.Height = Height
        ReDim Img.Elmt(Width - 1, Height - 1)

        For i = 0 To Width - 1
            For j = 0 To Height - 1
                Img.Elmt(i, j) = Elmt(i, j)
            Next
        Next
    End Sub
End Class

Public Class CCharacter
    Public PosX, PosY As Double
    Public Vx, Vy As Double
    Public FrameIndx As Integer
    Public CurrFrame As Integer
    Public ArrSprites() As CArrFrame
    Public IndxArrSprites As Integer
    Public FDir As FaceDir
    Public Destroy As Boolean = False

    Public Sub GetNextFrame()
        CurrFrame = CurrFrame + 1
        If CurrFrame = ArrSprites(IndxArrSprites).Elmt(FrameIndx).MaxFrameTime Then
            FrameIndx = FrameIndx + 1
            If FrameIndx = ArrSprites(IndxArrSprites).N Then
                FrameIndx = 0
            End If
            CurrFrame = 0
        End If
    End Sub

    Public Overridable Sub Update()

    End Sub
End Class

Public Class CCharBlastHornet
    Inherits CCharacter

    Public CurrState As StateBlastHornet
    Public startIntro As Boolean
    Dim upIntro As Boolean = False
    Public backToUp As Boolean = False
    Public newPointX, newPointY As Double
    Public initX, initY As Double

    Public Sub State(state As StateBlastHornet, indxspr As Integer)
        CurrState = state
        IndxArrSprites = indxspr
        CurrFrame = 0
        FrameIndx = 0
    End Sub

    Public Overrides Sub Update()
        Select Case CurrState
            Case StateBlastHornet.idle
                PosX = PosX + Vx
                PosY = PosY + Vy
                GetNextFrame()
                If startIntro = True Then
                    If PosY = 150 And upIntro = False Then
                        Vx = 0
                        Vy = -5
                        upIntro = True
                    ElseIf PosY <= 100 And upIntro = True Then
                        Vx = 0
                        Vy = 0
                        startIntro = False
                        upIntro = False
                        State(StateBlastHornet.intro, 0)
                    End If
                ElseIf startIntro = False Then
                    If backToUp = True Then
                        newPointX = initX
                        newPointY = initY
                        backToUp = False
                    End If

                    If Math.Abs(Vx) = 5 Then
                        If CInt(Math.Round(PosX)) - CInt((Math.Round(PosX) Mod 10)) = (CInt(Math.Round(newPointX)) - CInt(Math.Round(newPointX)) Mod 10) Then
                            Vx = 0
                            Vy = 0
                        End If
                    ElseIf Math.Abs(Vy) = 5 Then
                        If CInt(Math.Round(PosY)) - CInt((Math.Round(PosY) Mod 10)) = (CInt(Math.Round(newPointY)) - CInt(Math.Round(newPointY)) Mod 10) Then
                            Vx = 0
                            Vy = 0
                        End If
                    End If

                    If PosX <= 60 Then
                        FDir = FaceDir.Right
                        PosX = 60
                    ElseIf PosX >= 240 Then
                        FDir = FaceDir.Left
                        PosX = 240
                    End If

                    If PosY <= 65 Then
                        PosY = 65
                    ElseIf PosY >= 170 Then
                        PosY = 170
                    End If
                End If
            Case StateBlastHornet.intro
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.idle, 1)
                End If
            Case StateBlastHornet.hit
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.stingAttack, 10)
                    initX = PosX
                    initY = PosY
                    DDA(PosX, PosY, newPointX, newPointY)
                End If
            Case StateBlastHornet.stingAttack
                PosX = PosX + Vx
                PosY = PosY + Vy
                GetNextFrame()
                If PosY >= 170 Then
                    Vx = 0
                    Vy = 0
                    If initX > PosX Then
                        DDA(PosX, PosY, initX, initY)
                        backToUp = True
                        State(StateBlastHornet.hitWall, 3)
                    Else
                        DDA(PosX, PosY, initX, initY)
                        backToUp = True
                        State(StateBlastHornet.hitWall, 3)
                    End If
                End If
                If Math.Abs(Vx) = 5 Then
                    If CInt(Math.Round(PosX)) - CInt((Math.Round(PosX) Mod 10)) = (CInt(Math.Round(newPointX)) - CInt(Math.Round(newPointX)) Mod 10) Then
                        Vx = 0
                        Vy = 0
                        If initX > PosX Then
                            DDA(PosX, PosY, initX, initY)
                            backToUp = True
                            State(StateBlastHornet.hitWall, 3)
                        Else
                            DDA(PosX, PosY, initX, initY)
                            backToUp = True
                            State(StateBlastHornet.hitWall, 3)
                        End If
                    End If
                ElseIf Math.Abs(Vy) = 5 Then
                    If CInt(Math.Round(PosY)) - CInt((Math.Round(PosY) Mod 10)) = (CInt(Math.Round(newPointY)) - CInt(Math.Round(newPointY)) Mod 10) Then
                        Vx = 0
                        Vy = 0
                        If initX > PosX Then
                            DDA(PosX, PosY, initX, initY)
                            backToUp = True
                            State(StateBlastHornet.hitWall, 3)
                        Else
                            DDA(PosX, PosY, initX, initY)
                            backToUp = True
                            State(StateBlastHornet.hitWall, 3)
                        End If
                    End If
                End If
            Case StateBlastHornet.hitWall
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    If initX > PosX Then
                        FDir = FaceDir.Right
                    Else
                        FDir = FaceDir.Left
                    End If
                    State(StateBlastHornet.idle, 1)
                End If
            Case StateBlastHornet.introBomb
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.bomb, 5)
                End If
            Case StateBlastHornet.bomb
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.afterBomb, 6)
                End If
            Case StateBlastHornet.afterBomb
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.idle, 1)
                End If
            Case StateBlastHornet.shield
                GetNextFrame()
            Case StateBlastHornet.afterShield
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateBlastHornet.idle, 1)
                End If
            Case StateBlastHornet.death
                PosX = PosX + Vx
                PosY = PosY + Vy
                GetNextFrame()
                If PosY >= 175 Then
                    Vx = 0
                    Vy = 0
                Else
                    Vx = 0
                    Vy = 5
                End If
        End Select
    End Sub

    Public Sub DDA(ByVal x1 As Integer, ByVal y1 As Integer, ByVal x2 As Integer, ByVal y2 As Integer)
        Dim length, dx, dy As Double

        If Math.Abs(x2 - x1) > Math.Abs(y2 - y1) Then
            length = Math.Abs(x2 - x1)
        Else
            length = Math.Abs(y2 - y1)
        End If

        dx = ((x2 - x1) / length) * 5
        dy = ((y2 - y1) / length) * 5

        Vx = dx
        Vy = dy
    End Sub
End Class

Public Class CCharParasiteProjectile
    Inherits CCharacter

    Public CurrState As StateParasiteProjectile
    Public t As Double = 0
    Public parasiteTime As Double
    Public Vinitx, Vinity, Xinit, Yinit, g As Double
    Dim sticky As Boolean = False
    Public crosshairstick As Boolean = False
    Public dir As Double = Math.PI

    Public Sub State(state As StateParasiteProjectile, indxspr As Integer)
        CurrState = state
        IndxArrSprites = indxspr
        CurrFrame = 0
        FrameIndx = 0
    End Sub

    Public Overrides Sub Update()
        Xinit = PosX
        Yinit = PosY
        If FDir = FaceDir.Right Then
            Vinity = -0.7
        Else
            Vinity = -0.7
        End If

        g = 0.5
        Select Case CurrState
            Case StateParasiteProjectile.intro
                GetNextFrame()
                If Not (sticky) And Not crosshairstick Then
                    PosX = Xinit + Vinitx * t
                    PosY = Yinit + Vinity * t + 0.5 * g * t * t
                    t += 1
                    parasiteTime = t
                ElseIf Not (sticky) And crosshairstick Then
                    PosX = PosX + Vx
                    PosY = PosY + Vy
                    t += 1
                    parasiteTime = t
                Else
                    parasiteTime += 1
                End If
                If (PosX <= 30 Or PosX >= 270 Or PosY <= 30 Or PosY >= 200) And (parasiteTime >= t + 15) Then
                    Destroy = True
                    dir = Math.PI
                ElseIf PosX < 30 Then
                    PosX = 30
                    sticky = True
                ElseIf PosX > 270 Then
                    PosX = 270
                    sticky = True
                ElseIf PosY < 30 Then
                    PosY = 30
                    sticky = True
                ElseIf PosY > 200 Then
                    PosY = 200
                    sticky = True
                End If
        End Select
    End Sub
End Class

Public Class CrossHairProjectile
    Inherits CCharacter

    Public CurrState As StateCrossHairProjectile
    Public sticky As Boolean = False
    Public dir As Double = Math.PI

    Public Sub State(state As StateParasiteProjectile, indxspr As Integer)
        CurrState = state
        IndxArrSprites = indxspr
        CurrFrame = 0
        FrameIndx = 0
    End Sub

    Public Overrides Sub Update()
        Select Case CurrState
            Case StateCrossHairProjectile.find
                GetNextFrame()
                PosX = PosX + Vx
                PosY = PosY + Vy
                If CurrState = 0 And FrameIndx = 0 Then
                    If (PosX <= 30 Or PosX >= 270 Or PosY <= 30 Or PosY >= 200) Then
                        If Not (sticky) Then
                            Destroy = True
                            dir = Math.PI
                        End If
                    End If
                    If sticky Then
                        State(StateCrossHairProjectile.transit, 1)
                    End If
                End If
            Case StateCrossHairProjectile.transit
                If sticky = True Then
                    GetNextFrame()
                    State(StateCrossHairProjectile.lock, 2)
                End If
            Case StateCrossHairProjectile.lock
                GetNextFrame()
        End Select
    End Sub
End Class

Public Class CCharSpongeBob
    Inherits CCharacter

    Public CurrState As StateSpongeBob

    Public Sub State(state As StateSpongeBob, indxspr As Integer)
        CurrState = state
        IndxArrSprites = indxspr
        CurrFrame = 0
        FrameIndx = 0
    End Sub

    Public Overrides Sub Update()
        Select Case CurrState
            Case StateSpongeBob.intro
                GetNextFrame()
                PosX = PosX + Vx
                PosY = PosY + Vy
                If PosY >= 200 Then
                    Vx = 3
                    Vy = 0
                    State(StateSpongeBob.walk, 1)
                End If
            Case StateSpongeBob.walk
                GetNextFrame()
                PosX = PosX + Vx
                PosY = PosY + Vy
                If PosX <= 40 Then
                    FDir = FaceDir.Left
                    Vx = 3
                ElseIf PosX >= 240 Then
                    FDir = FaceDir.Right
                    Vx = -3
                End If

                If Rnd() < 0.03 Then
                    State(StateSpongeBob.bubble, 2)
                    Vx = 0
                    Vy = 0
                End If

            Case StateSpongeBob.bubble
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    State(StateSpongeBob.walk, 1)

                    If FDir = FaceDir.Left Then
                        Vx = 3
                    ElseIf FDir = FaceDir.Right Then
                        Vx = -3
                    End If
                End If

            Case StateSpongeBob.death
                GetNextFrame()
                If FrameIndx = 0 And CurrFrame = 0 Then
                    Destroy = True
                End If
        End Select
    End Sub
End Class

Public Class CCharBubbleProjectile
    Inherits CCharacter

    Public CurrState As StateBubbleProjectile
    Dim t As Integer = 0
    Public Vinitx, Vinity, Xinit, Yinit, g As Double

    Public Sub State(state As StateBubbleProjectile, indxspr As Integer)
        CurrState = state
        IndxArrSprites = indxspr
        CurrFrame = 0
        FrameIndx = 0
    End Sub

    Public Overrides Sub Update()
        Xinit = PosX
        Yinit = PosY
        If FDir = FaceDir.Right Then
            Vinity = -0.7
        Else
            Vinity = -0.7
        End If

        g = -0.5
        Select Case CurrState
            Case StateBubbleProjectile.attack
                GetNextFrame()
                PosX = Xinit + Vinitx * t
                PosY = Yinit + Vinity * t + 0.5 * g * t * t
                t += 1

                If (PosX <= 30 Or PosX >= 270 Or PosY <= 20 Or PosY >= 200) Then
                    Destroy = True
                ElseIf PosX < 30 Then
                    PosX = 30
                ElseIf PosX > 270 Then
                    PosX = 270
                ElseIf PosY < 20 Then
                    PosY = 20
                ElseIf PosY > 200 Then
                    PosY = 200
                End If
        End Select
    End Sub
End Class

Public Class CElmtFrame
    Public CtrPoint As TPoint
    Public Top, Bottom, Left, Right As Integer
    Public Indx As Integer
    Public MaxFrameTime As Integer

    Public Sub New(ctrx As Integer, Ctry As Integer, l As Integer, t As Integer, r As Integer, b As Integer, mft As Integer)
        CtrPoint.x = ctrx
        CtrPoint.y = Ctry
        Top = t
        Bottom = b
        Left = l
        Right = r
        MaxFrameTime = mft
    End Sub
End Class

Public Class CArrFrame
    Public N As Integer
    Public Elmt As CElmtFrame()

    Public Sub New()
        N = 0
        ReDim Elmt(-1)
    End Sub

    Public Overloads Sub Insert(E As CElmtFrame)
        ReDim Preserve Elmt(N)
        Elmt(N) = E
        N = N + 1
    End Sub

    Public Overloads Sub Insert(ctrx As Integer, ctry As Integer, l As Integer, t As Integer, r As Integer, b As Integer, mft As Integer)
        Dim E As CElmtFrame
        E = New CElmtFrame(ctrx, ctry, l, t, r, b, mft)
        ReDim Preserve Elmt(N)
        Elmt(N) = E
        N = N + 1
    End Sub
End Class

Public Structure TPoint
    Dim x As Integer
    Dim y As Integer
End Structure

