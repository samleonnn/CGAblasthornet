Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Public Class Sprite_Animation
    Dim bmp As Bitmap
    Dim bg, bg1, img As CImage
    Dim SpriteMap As CImage
    Dim SpriteMask As CImage
    Dim bhIntro, bhIdle, bhHit, bhHitWall, bhintroBomb, bhBomb, bhAfterBomb, bhShield, bhAfterShield, bhDeath, bhAttackStinger As CArrFrame
    Dim parasiteFly, crossHairFind, crossHairTransit, crossHairLock As CArrFrame
    Dim sbIntro, sbWalk, sbBubble, sbDeath As CArrFrame
    Dim bubbleAttack As CArrFrame
    Dim ListChar As New List(Of CCharacter)
    Dim BH As CCharBlastHornet
    Dim PP As CCharParasiteProjectile
    Dim SB As CCharSpongeBob
    Dim BB As CCharBubbleProjectile
    Dim CH As CrossHairProjectile

    Dim tr As Double = 2 * Math.PI / 180
    Dim V As Double = 5

    Sub resetCH()
        CH.Destroy = True
        CH.sticky = False
        CH.dir = Math.PI
        If PP IsNot Nothing Then
            PP.dir = Math.PI
        End If
        CH = Nothing
        chTimer.Enabled = False
    End Sub

    Private Sub chTimer_Tick(sender As Object, e As EventArgs) Handles chTimer.Tick
        resetCH()
    End Sub

    Private Sub sbTimer_Tick(sender As Object, e As EventArgs) Handles sbTimer.Tick
        respawnSB()
        sbTimer.Enabled = False
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim CC As CCharacter

        PictureBox1.Refresh()

        For Each CC In ListChar
            CC.Update()
        Next

        If BH.CurrState = StateBlastHornet.bomb And BH.CurrFrame = 0 And BH.FrameIndx = 0 Then
            If CH IsNot Nothing Then
                If CH.sticky Then
                    createParasite(0)
                Else
                    createParasite(0.05)
                    createParasite(0.5)
                    createParasite(0.95)
                End If
            Else
                createParasite(0.05)
                createParasite(0.5)
                createParasite(0.95)
            End If
        End If

        If BH.startIntro = False And BH.backToUp = False Then
            If SB.PosX < BH.PosX And BH.CurrState = StateBlastHornet.idle Then
                BH.FDir = FaceDir.Left
            ElseIf SB.PosX > BH.PosX And BH.CurrState = StateBlastHornet.idle Then
                BH.FDir = FaceDir.Right
            End If
        End If

        If CH IsNot Nothing Then
            calculateCrosshair(CH)
            If collision(CH, SB) Then
                CH.sticky = True
                chTimer.Enabled = True
            End If
            If CH.sticky Then
                CH.PosX = SB.PosX
                CH.PosY = SB.PosY - 5
                If PP IsNot Nothing Then
                    calculateParasiteCross(PP)
                End If
            End If
        End If

        If SB.CurrState = StateSpongeBob.bubble Then
            createBubble()
        End If

        If collision(BH, SB) And SB.CurrState <> StateSpongeBob.death Then
            SB.State(StateSpongeBob.death, 3)
            sbTimer.Enabled = True

            If CH IsNot Nothing Then
                resetCH()
            End If

        ElseIf PP IsNot Nothing And collision(PP, SB) And SB.CurrState <> StateSpongeBob.death Then
            SB.State(StateSpongeBob.death, 3)
            sbTimer.Enabled = True
            PP.Destroy = True
            PP.crosshairstick = False

            If CH IsNot Nothing Then
                resetCH()
            End If
        End If

        Dim tempListChar As New List(Of CCharacter)

        For Each CC In ListChar
            If Not CC.Destroy Then
                tempListChar.Add(CC)
            End If
        Next

        ListChar = tempListChar
        DisplayImg()
    End Sub

    Sub calculateCrosshair(ByVal dordor As CrossHairProjectile)
        Dim vx, vy As Double
        Dim dx, dy, z As Double

        vx = V * Math.Cos(dordor.dir)
        vy = V * Math.Sin(dordor.dir)

        dx = SB.PosX - dordor.PosX
        dy = SB.PosY - dordor.PosY

        z = vx * dx - vy * dy

        If z >= 0 Then
            dordor.dir = dordor.dir + 30 * tr
        Else
            dordor.dir = dordor.dir - 30 * tr
        End If

        vx = V * Math.Cos(dordor.dir)
        vy = V * Math.Sin(dordor.dir)

        If SB.PosX < dordor.PosX Then
            dordor.Vx = -vx
        Else
            dordor.Vx = vx
        End If

        dordor.Vy = Math.Abs(vy)
    End Sub

    Sub calculateParasiteCross(ByVal parasite As CCharParasiteProjectile)
        Dim vx, vy As Double
        Dim dx, dy, z As Double

        vx = V * Math.Cos(parasite.dir)
        vy = V * Math.Sin(parasite.dir)

        dx = CH.PosX - parasite.PosX
        dy = CH.PosY - parasite.PosY

        z = vx * dx - vy * dy

        If z >= 0 Then
            parasite.dir = parasite.dir + 30 * tr
        Else
            parasite.dir = parasite.dir - 30 * tr
        End If

        vx = V * Math.Cos(parasite.dir)
        vy = V * Math.Sin(parasite.dir)

        If SB.PosX < parasite.PosX Then
            parasite.Vx = -vx
            parasite.FDir = FaceDir.Right
        Else
            parasite.Vx = vx
            parasite.FDir = FaceDir.Left
        End If

        parasite.Vy = Math.Abs(vy)
    End Sub

    Sub respawnSB()
        Dim BHFrame As CElmtFrame = BH.ArrSprites(BH.IndxArrSprites).Elmt(BH.FrameIndx)
        Dim spriteWidth = BHFrame.Right - BHFrame.Left
        Dim spriteLeft, spriteRight As Integer
        Dim newPosX As Integer = GetRandom(40, 240)

        If BH.FDir = FaceDir.Left Then
            spriteLeft = BH.PosX - BHFrame.CtrPoint.x + BHFrame.Left
            spriteRight = spriteLeft + spriteWidth

        Else
            spriteLeft = BH.PosX + BHFrame.CtrPoint.x - BHFrame.Right
            spriteRight = spriteLeft + spriteWidth

        End If

        Do While newPosX > spriteLeft - 20 And newPosX < spriteRight + 20
            newPosX = GetRandom(40, 240)
        Loop

        If newPosX < spriteLeft - 20 Then
            SB.PosX = newPosX
            SB.FDir = FaceDir.Left
            SB.Destroy = False
            SB.PosY = 50
            SB.Vx = 0
            SB.Vy = 5
            SB.State(StateSpongeBob.intro, 0)

            ListChar.Add(SB)

        ElseIf newPosX > spriteRight + 20 Then
            SB.PosX = newPosX
            SB.FDir = FaceDir.Right
            SB.Destroy = False
            SB.PosY = 50
            SB.Vx = 0
            SB.Vy = 5
            SB.State(StateSpongeBob.intro, 0)

            ListChar.Add(SB)
        End If
    End Sub

    Public Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer
        Static Generator As System.Random = New System.Random()
        Return Generator.Next(Min, Max)
    End Function

    Sub createParasite(ByVal V0X As Double)
        PP = New CCharParasiteProjectile
        If BH.FDir = FaceDir.Left Then
            PP.PosX = BH.PosX - 24
            PP.Vinitx = -V0X
            PP.FDir = FaceDir.Right
        Else
            PP.PosX = BH.PosX + 24
            PP.Vinitx = V0X
            PP.FDir = FaceDir.Left
        End If

        PP.PosY = BH.PosY - 1

        PP.Vx = 0
        PP.Vy = 0

        If CH IsNot Nothing Then
            If CH.sticky Then
                calculateParasiteCross(PP)
                PP.crosshairstick = True
            End If
        End If

        PP.CurrState = StateParasiteProjectile.intro
        ReDim PP.ArrSprites(0)
        PP.ArrSprites(0) = parasiteFly

        ListChar.Add(PP)
    End Sub

    Sub createCrossHair()
        CH = New CrossHairProjectile
        If BH.FDir = FaceDir.Left Then
            CH.PosX = BH.PosX - 24
        Else
            CH.PosX = BH.PosX + 24
        End If

        CH.PosY = BH.PosY - 1

        CH.CurrState = StateCrossHairProjectile.find
        ReDim CH.ArrSprites(2)
        CH.ArrSprites(0) = crossHairFind
        CH.ArrSprites(1) = crossHairTransit
        CH.ArrSprites(2) = crossHairLock

        ListChar.Add(CH)
    End Sub

    Sub createBubble()
        BB = New CCharBubbleProjectile
        Dim V0X As Double = 0.5

        If SB.FDir = FaceDir.Left Then
            BB.PosX = SB.PosX + 24
            BB.Vinitx = V0X
            BB.FDir = FaceDir.Right
        Else
            BB.PosX = SB.PosX - 24
            BB.Vinitx = -V0X
            BB.FDir = FaceDir.Left
        End If

        BB.PosY = SB.PosY - 10

        BB.Vx = 0
        BB.Vy = 0
        BB.CurrState = StateBubbleProjectile.attack
        ReDim BB.ArrSprites(0)
        BB.ArrSprites(0) = bubbleAttack

        ListChar.Add(BB)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        bg = New CImage
        bg.OpenImage("./24bit/bg_m.bmp")
        bg.CopyImg(img)
        bg.CopyImg(bg1)

        SpriteMap = New CImage
        SpriteMap.OpenImage("./24bit/spt.bmp")
        SpriteMap.CreateMask(SpriteMask)

        bhIntro = New CArrFrame
        'baris 9
        bhIntro.Insert(40, 759, 13, 712, 67, 803, 1)
        bhIntro.Insert(111, 759, 72, 712, 149, 803, 1)
        bhIntro.Insert(200, 759, 151, 730, 250, 803, 1)
        'baris 9
        bhIntro.Insert(284, 759, 256, 713, 310, 808, 1)
        bhIntro.Insert(353, 759, 313, 714, 395, 808, 1)
        bhIntro.Insert(446, 759, 396, 732, 495, 808, 1)
        'baris 10
        bhIntro.Insert(42, 858, 13, 813, 67, 912, 1)
        bhIntro.Insert(109, 858, 68, 814, 145, 912, 1)
        bhIntro.Insert(198, 858, 147, 832, 246, 912, 1)
        'baris 10
        bhIntro.Insert(280, 858, 252, 814, 303, 912, 1)
        bhIntro.Insert(350, 858, 307, 814, 389, 912, 1)
        bhIntro.Insert(443, 858, 390, 832, 489, 912, 1)
        'baris 11
        bhIntro.Insert(109, 961, 80, 916, 130, 1015, 1)
        bhIntro.Insert(179, 961, 134, 916, 211, 1015, 1)
        bhIntro.Insert(271, 961, 216, 935, 315, 1015, 1)
        'baris 9
        bhIntro.Insert(40, 759, 13, 712, 67, 803, 1)
        bhIntro.Insert(111, 759, 72, 712, 149, 803, 1)
        bhIntro.Insert(200, 759, 151, 730, 250, 803, 1)
        'baris 2
        bhIntro.Insert(37, 142, 13, 97, 63, 184, 1)
        bhIntro.Insert(105, 142, 65, 97, 142, 184, 1)
        bhIntro.Insert(192, 142, 142, 115, 241, 184, 1)
        'baris 2
        bhIntro.Insert(270, 142, 245, 97, 295, 181, 1)
        bhIntro.Insert(338, 142, 297, 98, 374, 181, 1)
        bhIntro.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 3
        bhIntro.Insert(40, 232, 14, 187, 64, 267, 1)
        bhIntro.Insert(109, 232, 67, 190, 144, 267, 1)
        bhIntro.Insert(198, 232, 146, 208, 245, 267, 1)
        'baris 3
        bhIntro.Insert(272, 232, 246, 188, 296, 267, 1)
        bhIntro.Insert(339, 232, 297, 188, 374, 267, 1)
        bhIntro.Insert(426, 232, 374, 206, 473, 267, 1)
        'baris 4: jrum dikit
        bhIntro.Insert(39, 318, 13, 273, 63, 353, 1)
        bhIntro.Insert(107, 318, 65, 274, 142, 353, 1)
        bhIntro.Insert(196, 318, 144, 292, 241, 353, 1)
        'baris 4: jarum nongol
        bhIntro.Insert(272, 318, 246, 273, 296, 356, 1)
        bhIntro.Insert(339, 318, 297, 274, 374, 356, 1)
        bhIntro.Insert(427, 318, 375, 292, 474, 356, 1)
        'baris 5: splash 1
        bhIntro.Insert(39, 401, 13, 356, 63, 439, 1)
        bhIntro.Insert(106, 401, 66, 357, 141, 439, 1)
        bhIntro.Insert(195, 401, 143, 375, 242, 439, 1)
        'baris 4: jarum nongol
        bhIntro.Insert(272, 318, 246, 273, 296, 356, 1)
        bhIntro.Insert(339, 318, 297, 274, 374, 356, 1)
        bhIntro.Insert(427, 318, 375, 292, 474, 356, 1)
        'baris 4: jrum dikit
        bhIntro.Insert(39, 318, 13, 273, 63, 353, 1)
        bhIntro.Insert(107, 318, 65, 274, 142, 353, 1)
        bhIntro.Insert(196, 318, 144, 292, 241, 353, 1)
        'baris 3
        bhIntro.Insert(40, 232, 14, 187, 64, 267, 1)
        bhIntro.Insert(109, 232, 67, 190, 144, 267, 1)
        bhIntro.Insert(198, 232, 146, 208, 245, 267, 1)
        'baris 2
        bhIntro.Insert(270, 142, 245, 97, 295, 181, 1)
        bhIntro.Insert(338, 142, 297, 98, 374, 181, 1)
        bhIntro.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 2
        bhIntro.Insert(37, 142, 13, 97, 63, 184, 1)
        bhIntro.Insert(105, 142, 65, 97, 142, 184, 1)
        bhIntro.Insert(192, 142, 142, 115, 241, 184, 1)

        bhIdle = New CArrFrame
        bhIdle.Insert(89, 49, 64, 1, 117, 94, 1)
        bhIdle.Insert(158, 49, 120, 2, 197, 94, 1)
        bhIdle.Insert(246, 49, 198, 19, 297, 94, 1)

        bhHit = New CArrFrame
        'baris 2
        bhHit.Insert(37, 142, 13, 97, 63, 184, 1)
        bhHit.Insert(105, 142, 65, 97, 142, 184, 1)
        bhHit.Insert(192, 142, 142, 115, 241, 184, 1)
        'baris 2
        bhHit.Insert(270, 142, 245, 97, 295, 181, 1)
        bhHit.Insert(338, 142, 297, 98, 374, 181, 1)
        bhHit.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 3
        bhHit.Insert(40, 232, 14, 187, 64, 267, 1)
        bhHit.Insert(109, 232, 67, 190, 144, 267, 1)
        bhHit.Insert(198, 232, 146, 208, 245, 267, 1)
        'baris 3
        bhHit.Insert(272, 232, 246, 188, 296, 267, 1)
        bhHit.Insert(339, 232, 297, 188, 374, 267, 1)
        bhHit.Insert(426, 232, 374, 206, 473, 267, 1)
        'baris 4: jrum dikit
        bhHit.Insert(39, 318, 13, 273, 63, 353, 1)
        bhHit.Insert(107, 318, 65, 274, 142, 353, 1)
        bhHit.Insert(196, 318, 144, 292, 241, 353, 1)
        'baris 4: jarum nongol
        bhHit.Insert(272, 318, 246, 273, 296, 356, 1)
        bhHit.Insert(339, 318, 297, 274, 374, 356, 1)
        bhHit.Insert(427, 318, 375, 292, 474, 356, 1)
        'baris 5: splash 1
        bhHit.Insert(39, 401, 13, 356, 63, 439, 1)
        bhHit.Insert(106, 401, 66, 357, 141, 439, 1)
        bhHit.Insert(195, 401, 143, 375, 242, 439, 1)
        'baris 5: after splash
        bhHit.Insert(274, 401, 248, 356, 298, 439, 1)
        bhHit.Insert(342, 401, 300, 357, 377, 439, 1)
        bhHit.Insert(342, 401, 300, 357, 377, 439, 1)
        'baris 6: blink +
        bhHit.Insert(40, 489, 12, 444, 64, 528, 1)
        bhHit.Insert(110, 489, 68, 445, 145, 528, 1)
        bhHit.Insert(199, 489, 147, 463, 246, 528, 1)
        'baris 6: blink x
        bhHit.Insert(277, 489, 250, 444, 301, 527, 1)
        bhHit.Insert(346, 489, 304, 445, 381, 527, 1)
        bhHit.Insert(436, 489, 384, 463, 483, 527, 1)
        'baris 7: small ball
        bhHit.Insert(40, 580, 14, 535, 64, 618, 1)
        bhHit.Insert(109, 580, 67, 536, 149, 618, 1)
        bhHit.Insert(205, 580, 153, 554, 252, 618, 1)
        'baris 7: ring
        bhHit.Insert(285, 580, 258, 535, 309, 618, 1)
        bhHit.Insert(355, 580, 313, 536, 390, 618, 1)
        bhHit.Insert(444, 580, 392, 554, 491, 618, 1)
        'baris 8: ring
        bhHit.Insert(143, 667, 115, 622, 167, 706, 1)
        bhHit.Insert(212, 667, 170, 623, 247, 706, 1)
        bhHit.Insert(302, 667, 250, 641, 349, 706, 1)
        'baris 4: jarum nongol
        bhHit.Insert(272, 318, 246, 273, 296, 356, 1)
        bhHit.Insert(339, 318, 297, 274, 374, 356, 1)
        bhHit.Insert(427, 318, 375, 292, 474, 356, 1)

        bhAttackStinger = New CArrFrame
        'baris 4: jarum nongol
        bhAttackStinger.Insert(272, 318, 246, 273, 296, 356, 1)
        bhAttackStinger.Insert(339, 318, 297, 274, 374, 356, 1)
        bhAttackStinger.Insert(427, 318, 375, 292, 474, 356, 1)

        bhHitWall = New CArrFrame
        'baris 4: jarum nongol
        bhHitWall.Insert(272, 318, 246, 273, 296, 356, 1)
        bhHitWall.Insert(339, 318, 297, 274, 374, 356, 1)
        bhHitWall.Insert(427, 318, 375, 292, 474, 356, 1)
        'baris 4: jrum dikit
        bhHitWall.Insert(39, 318, 13, 273, 63, 353, 1)
        bhHitWall.Insert(107, 318, 65, 274, 142, 353, 1)
        bhHitWall.Insert(196, 318, 144, 292, 241, 353, 1)
        'baris 3
        bhHitWall.Insert(40, 232, 14, 187, 64, 267, 1)
        bhHitWall.Insert(109, 232, 67, 190, 144, 267, 1)
        bhHitWall.Insert(198, 232, 146, 208, 245, 267, 1)
        'baris 2
        bhHitWall.Insert(270, 142, 245, 97, 295, 181, 1)
        bhHitWall.Insert(338, 142, 297, 98, 374, 181, 1)
        bhHitWall.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 2
        bhHitWall.Insert(37, 142, 13, 97, 63, 184, 1)
        bhHitWall.Insert(105, 142, 65, 97, 142, 184, 1)
        bhHitWall.Insert(192, 142, 142, 115, 241, 184, 1)

        bhintroBomb = New CArrFrame
        'baris 9
        bhintroBomb.Insert(40, 759, 13, 712, 67, 803, 1)
        bhintroBomb.Insert(111, 759, 72, 712, 149, 803, 1)
        bhintroBomb.Insert(200, 759, 151, 730, 250, 803, 1)
        'baris 9
        bhintroBomb.Insert(284, 759, 256, 713, 310, 808, 1)
        bhintroBomb.Insert(353, 759, 313, 714, 395, 808, 1)
        bhintroBomb.Insert(446, 759, 396, 732, 495, 808, 1)
        'baris 10
        bhintroBomb.Insert(42, 858, 13, 813, 67, 912, 1)
        bhintroBomb.Insert(109, 858, 68, 814, 145, 912, 1)
        bhintroBomb.Insert(198, 858, 147, 832, 246, 912, 1)
        'baris 10
        bhintroBomb.Insert(280, 858, 252, 814, 303, 912, 1)
        bhintroBomb.Insert(350, 858, 307, 814, 389, 912, 1)
        bhintroBomb.Insert(443, 858, 390, 832, 489, 912, 1)

        bhBomb = New CArrFrame
        'baris 11
        bhBomb.Insert(109, 961, 80, 916, 130, 1015, 1)
        bhBomb.Insert(179, 961, 134, 916, 211, 1015, 1)
        bhBomb.Insert(271, 961, 216, 935, 315, 1015, 1)

        bhAfterBomb = New CArrFrame
        'baris 9.2
        bhAfterBomb.Insert(284, 759, 256, 713, 310, 808, 1)
        bhAfterBomb.Insert(353, 759, 313, 714, 395, 808, 1)
        bhAfterBomb.Insert(446, 759, 396, 732, 495, 808, 1)
        'baris 9.1
        bhAfterBomb.Insert(40, 759, 13, 712, 67, 803, 1)
        bhAfterBomb.Insert(111, 759, 72, 712, 149, 803, 1)
        bhAfterBomb.Insert(200, 759, 151, 730, 250, 803, 1)

        bhShield = New CArrFrame
        'baris 2
        bhShield.Insert(37, 142, 13, 97, 63, 184, 1)
        bhShield.Insert(105, 142, 65, 97, 142, 184, 1)
        bhShield.Insert(192, 142, 142, 115, 241, 184, 1)
        'baris 2
        bhShield.Insert(270, 142, 245, 97, 295, 181, 1)
        bhShield.Insert(338, 142, 297, 98, 374, 181, 1)
        bhShield.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 3
        bhShield.Insert(40, 232, 14, 187, 64, 267, 1)
        bhShield.Insert(109, 232, 67, 190, 144, 267, 1)
        bhShield.Insert(198, 232, 146, 208, 245, 267, 1)
        'baris 3
        bhShield.Insert(272, 232, 246, 188, 296, 267, 1)
        bhShield.Insert(339, 232, 297, 188, 374, 267, 1)
        bhShield.Insert(426, 232, 374, 206, 473, 267, 1)

        bhAfterShield = New CArrFrame
        'baris 2.2
        bhAfterShield.Insert(270, 142, 245, 97, 295, 181, 1)
        bhAfterShield.Insert(338, 142, 297, 98, 374, 181, 1)
        bhAfterShield.Insert(425, 142, 375, 116, 473, 181, 1)
        'baris 2.1
        bhAfterShield.Insert(37, 142, 13, 97, 63, 184, 1)
        bhAfterShield.Insert(105, 142, 65, 97, 142, 184, 1)
        bhAfterShield.Insert(192, 142, 142, 115, 241, 184, 1)

        bhDeath = New CArrFrame
        bhDeath.Insert(373, 957, 321, 941, 420, 992, 1)

        BH = New CCharBlastHornet
        ReDim BH.ArrSprites(10)
        BH.ArrSprites(0) = bhIntro
        BH.ArrSprites(1) = bhIdle
        BH.ArrSprites(2) = bhHit
        BH.ArrSprites(3) = bhHitWall
        BH.ArrSprites(4) = bhintroBomb
        BH.ArrSprites(5) = bhBomb
        BH.ArrSprites(6) = bhAfterBomb
        BH.ArrSprites(7) = bhShield
        BH.ArrSprites(8) = bhAfterShield
        BH.ArrSprites(9) = bhDeath
        BH.ArrSprites(10) = bhAttackStinger

        BH.PosX = 235
        BH.PosY = 50
        BH.Vx = 0
        BH.Vy = 5
        BH.startIntro = True
        BH.State(StateBlastHornet.idle, 1)
        BH.FDir = FaceDir.Left

        ListChar.Add(BH)

        'Parasite Projectile
        parasiteFly = New CArrFrame
        parasiteFly.Insert(13, 1044, 3, 1031, 25, 1056, 1)
        parasiteFly.Insert(38, 1044, 27, 1033, 52, 1056, 1)
        parasiteFly.Insert(64, 1044, 54, 1034, 76, 1056, 1)

        'Bubble Attack Projectile
        bubbleAttack = New CArrFrame
        bubbleAttack.Insert(382, 1361, 373, 1352, 391, 1371, 4)
        bubbleAttack.Insert(403, 1360, 394, 1350, 412, 1370, 4)
        bubbleAttack.Insert(424, 1359, 415, 1348, 432, 1371, 4)

        'CrossHair Projectile
        crossHairFind = New CArrFrame
        crossHairFind.Insert(99, 1045, 91, 1037, 107, 1053, 1)
        crossHairFind.Insert(118, 1045, 110, 1037, 126, 1053, 1)

        crossHairTransit = New CArrFrame
        crossHairTransit.Insert(145, 1045, 133, 1033, 157, 1056, 7)
        crossHairTransit.Insert(173, 1045, 161, 1033, 185, 1057, 7)

        crossHairLock = New CArrFrame
        crossHairLock.Insert(204, 1045, 188, 1029, 220, 1061, 1)
        crossHairLock.Insert(242, 1045, 226, 1029, 258, 1061, 1)

        'Dummy Character
        sbIntro = New CArrFrame
        sbIntro.Insert(388, 1494, 364, 1469, 407, 1508, 1)
        sbIntro.Insert(433, 1493, 409, 1466, 452, 1508, 1)

        sbWalk = New CArrFrame
        sbWalk.Insert(98, 1298, 75, 1273, 115, 1312, 1)
        sbWalk.Insert(142, 1298, 117, 1273, 158, 1311, 1)
        sbWalk.Insert(183, 1298, 161, 1274, 200, 1311, 1)
        sbWalk.Insert(222, 1298, 205, 1273, 237, 1312, 1)
        sbWalk.Insert(256, 1298, 239, 1273, 268, 1313, 1)
        sbWalk.Insert(289, 1298, 271, 1273, 300, 1312, 1)
        sbWalk.Insert(324, 1299, 306, 1274, 334, 1312, 1)
        sbWalk.Insert(355, 1299, 338, 1275, 368, 1312, 1)
        sbWalk.Insert(387, 1300, 370, 1275, 404, 1314, 1)

        sbBubble = New CArrFrame
        sbBubble.Insert(162, 1369, 137, 1337, 185, 1382, 1)
        sbBubble.Insert(211, 1369, 186, 1334, 234, 1382, 1)
        sbBubble.Insert(261, 1370, 236, 1338, 283, 1383, 1)
        sbBubble.Insert(309, 1369, 285, 1334, 330, 1382, 1)

        sbDeath = New CArrFrame
        sbDeath.Insert(94, 1427, 69, 1402, 113, 1440, 2)
        sbDeath.Insert(138, 1427, 115, 1402, 156, 1440, 2)
        sbDeath.Insert(181, 1428, 158, 1404, 200, 1440, 2)
        sbDeath.Insert(229, 1431, 202, 1418, 248, 1440, 2)
        sbDeath.Insert(275, 1431, 250, 1418, 293, 1440, 2)
        sbDeath.Insert(326, 1430, 295, 1421, 345, 1440, 2)
        sbDeath.Insert(387, 1429, 356, 1423, 406, 1439, 2)

        SB = New CCharSpongeBob
        ReDim SB.ArrSprites(3)
        SB.ArrSprites(0) = sbIntro
        SB.ArrSprites(1) = sbWalk
        SB.ArrSprites(2) = sbBubble
        SB.ArrSprites(3) = sbDeath

        SB.PosX = 50
        SB.PosY = 50
        SB.Vx = 0
        SB.Vy = 5
        SB.State(StateSpongeBob.intro, 0)
        SB.FDir = FaceDir.Left

        ListChar.Add(SB)

        bmp = New Bitmap(img.Width, img.Height)

        'Display Image
        DisplayImg()
        'Resize Image
        ResizeImg()

        Me.MaximizeBox = False

        Timer1.Enabled = True
    End Sub

    Sub PutSprite()
        Dim i, j As Integer
        For i = 0 To img.Width - 1
            For j = 0 To img.Height - 1
                img.Elmt(i, j) = bg1.Elmt(i, j)
            Next
        Next

        For Each c In ListChar
            Dim EF As CElmtFrame = c.ArrSprites(c.IndxArrSprites).Elmt(c.FrameIndx)
            Dim spriteWidth = EF.Right - EF.Left
            Dim spriteHeight = EF.Bottom - EF.Top

            If c.FDir = FaceDir.Left Then
                Dim spriteLeft As Integer = c.PosX - EF.CtrPoint.x + EF.Left
                Dim spriteTop As Integer = c.PosY - EF.CtrPoint.y + EF.Top

                For i = 0 To spriteWidth
                    For j = 0 To spriteHeight
                        img.Elmt(spriteLeft + i, spriteTop + j) = opAND(img.Elmt(spriteLeft + i, spriteTop + j), SpriteMask.Elmt(EF.Left + i, EF.Top + j))
                    Next
                Next

                For i = 0 To spriteWidth
                    For j = 0 To spriteHeight
                        img.Elmt(spriteLeft + i, spriteTop + j) = opOR(img.Elmt(spriteLeft + i, spriteTop + j), SpriteMap.Elmt(EF.Left + i, EF.Top + j))
                    Next
                Next
            Else
                Dim spriteLeft = c.PosX + EF.CtrPoint.x - EF.Right
                Dim spriteTop = c.PosY - EF.CtrPoint.y + EF.Top

                For i = 0 To spriteWidth
                    For j = 0 To spriteHeight
                        img.Elmt(spriteLeft + i, spriteTop + j) = opAND(img.Elmt(spriteLeft + i, spriteTop + j), SpriteMask.Elmt(EF.Right - i, EF.Top + j))
                    Next
                Next

                For i = 0 To spriteWidth
                    For j = 0 To spriteHeight
                        img.Elmt(spriteLeft + i, spriteTop + j) = opOR(img.Elmt(spriteLeft + i, spriteTop + j), SpriteMap.Elmt(EF.Right - i, EF.Top + j))
                    Next
                Next
            End If
        Next
    End Sub

    Sub DisplayImg()
        Dim i, j As Integer
        PutSprite()

        Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
        Dim bmpdata As System.Drawing.Imaging.BitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)

        Dim ptr As IntPtr = bmpdata.Scan0
        Dim bytes As Integer = Math.Abs(bmpdata.Stride) * bmp.Height
        Dim rgbvalues(bytes) As Byte

        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, bytes)

        Dim n As Integer = 0
        Dim col As System.Drawing.Color

        For j = 0 To img.Height - 1
            For i = 0 To img.Width - 1
                col = img.Elmt(i, j)
                rgbvalues(n) = col.B
                rgbvalues(n + 1) = col.G
                rgbvalues(n + 2) = col.R
                rgbvalues(n + 3) = col.A

                n = n + 4
            Next
        Next

        System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr, bytes)

        bmp.UnlockBits(bmpdata)

        PictureBox1.Refresh()
        PictureBox1.Image = bmp
        PictureBox1.Width = bmp.Width
        PictureBox1.Height = bmp.Height
        PictureBox1.Top = 0
        PictureBox1.Left = 0
    End Sub

    Sub ResizeImg()
        Dim w, h As Integer

        w = PictureBox1.Width
        h = PictureBox1.Height

        Me.ClientSize = New Size(w, h)
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        If BH.CurrState = StateBlastHornet.idle And BH.startIntro = False And e.Button = MouseButtons.Left Then
            If e.X <= 60 And e.Y <= 65 Then
                BH.newPointX = 60
                BH.newPointY = 65
                DDA(BH.PosX, BH.PosY, 60, 65, BH)
            ElseIf e.X >= 240 And e.Y <= 65 Then
                BH.newPointX = 240
                BH.newPointY = 65
                DDA(BH.PosX, BH.PosY, 240, 65, BH)
            ElseIf e.X <= 60 And e.Y >= 170 Then
                BH.newPointX = 60
                BH.newPointY = 170
                DDA(BH.PosX, BH.PosY, 60, 170, BH)
            ElseIf e.X >= 240 And e.Y >= 170 Then
                BH.newPointX = 240
                BH.newPointY = 170
                DDA(BH.PosX, BH.PosY, 240, 170, BH)
            ElseIf e.X <= 60 Then
                BH.newPointX = 60
                BH.newPointY = e.Y
                DDA(BH.PosX, BH.PosY, 60, e.Y, BH)
            ElseIf e.X >= 240 Then
                BH.newPointX = 240
                BH.newPointY = e.Y
                DDA(BH.PosX, BH.PosY, 240, e.Y, BH)
            ElseIf e.Y <= 65 Then
                BH.newPointX = e.X
                BH.newPointY = 65
                DDA(BH.PosX, BH.PosY, e.X, 65, BH)
            ElseIf e.Y >= 170 Then
                BH.newPointX = e.X
                BH.newPointY = 170
                DDA(BH.PosX, BH.PosY, e.X, 170, BH)
            Else
                BH.newPointX = e.X
                BH.newPointY = e.Y
                DDA(BH.PosX, BH.PosY, e.X, e.Y, BH)
            End If
            If e.X < BH.PosX Then
                BH.FDir = FaceDir.Left
            ElseIf e.X > BH.PosX Then
                BH.FDir = FaceDir.Right
            End If
        End If

        If BH.startIntro = False And BH.CurrState = StateBlastHornet.idle And e.Button = MouseButtons.Right Then
            If e.X < BH.PosX Then
                BH.FDir = FaceDir.Left
            ElseIf e.X > BH.PosX Then
                BH.FDir = FaceDir.Right
            End If
            BH.State(StateBlastHornet.introBomb, 4)
        End If
    End Sub

    Public Sub DDA(ByVal x1 As Integer, ByVal y1 As Integer, ByVal x2 As Integer, ByVal y2 As Integer, ByVal ch As CCharacter)
        Dim length, dx, dy As Double

        If Math.Abs(x2 - x1) > Math.Abs(y2 - y1) Then
            length = Math.Abs(x2 - x1)
        Else
            length = Math.Abs(y2 - y1)
        End If

        dx = ((x2 - x1) / length) * 5
        dy = ((y2 - y1) / length) * 5

        ch.Vx = dx
        ch.Vy = dy
    End Sub

    Private Sub Sprite_Animation_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Z And BH.startIntro = False And BH.CurrState = StateBlastHornet.idle Then
            BH.newPointX = SB.PosX
            BH.newPointY = SB.PosY
            BH.State(StateBlastHornet.hit, 2)
        End If
        If e.KeyCode = Keys.X And BH.startIntro = False And BH.CurrState = StateBlastHornet.idle Then
            If CH IsNot Nothing Then
                resetCH()
            End If
            createCrossHair()
            chTimer.Enabled = False
        End If
    End Sub

    Public Function collision(ByVal char1 As CCharacter, ByVal dummy As CCharSpongeBob) As Boolean
        Dim spriteLeft, spriteTop, spriteRight, spriteBottom As Integer
        Dim spriteLeftDummy, spriteTopDummy, spriteRightDummy, spriteBottomDummy As Integer

        For Each c In ListChar
            If c Is char1 Then
                Dim EF As CElmtFrame = c.ArrSprites(c.IndxArrSprites).Elmt(c.FrameIndx)
                Dim spriteWidth = EF.Right - EF.Left
                Dim spriteHeight = EF.Bottom - EF.Top

                If c IsNot BH Then
                    If c.FDir = FaceDir.Left Then
                        spriteLeft = c.PosX - EF.CtrPoint.x + EF.Left
                        spriteTop = c.PosY - EF.CtrPoint.y + EF.Top
                        spriteRight = spriteLeft + spriteWidth
                        spriteBottom = spriteTop + spriteHeight
                    Else
                        spriteLeft = c.PosX + EF.CtrPoint.x - EF.Right
                        spriteTop = c.PosY - EF.CtrPoint.y + EF.Top
                        spriteRight = spriteLeft + spriteWidth
                        spriteBottom = spriteTop + spriteHeight
                    End If

                ElseIf c Is BH Then
                    If c.FDir = FaceDir.Left Then
                        spriteLeft = c.PosX - 23
                        spriteTop = c.PosY - EF.CtrPoint.y + EF.Top
                        spriteRight = c.PosX + 23
                        spriteBottom = spriteTop + spriteHeight - 3
                    Else
                        spriteLeft = c.PosX - 23
                        spriteTop = c.PosY - EF.CtrPoint.y + EF.Top
                        spriteRight = c.PosX + 23
                        spriteBottom = spriteTop + spriteHeight - 3
                    End If
                End If

            ElseIf c Is dummy Then
                Dim EF As CElmtFrame = c.ArrSprites(c.IndxArrSprites).Elmt(c.FrameIndx)
                Dim spriteWidth = EF.Right - EF.Left
                Dim spriteHeight = EF.Bottom - EF.Top

                If c.FDir = FaceDir.Left Then
                    spriteLeftDummy = c.PosX - EF.CtrPoint.x + EF.Left
                    spriteTopDummy = c.PosY - EF.CtrPoint.y + EF.Top
                    spriteRightDummy = spriteLeftDummy + spriteWidth
                    spriteBottomDummy = spriteTopDummy + spriteHeight
                Else
                    spriteLeftDummy = c.PosX + EF.CtrPoint.x - EF.Right
                    spriteTopDummy = c.PosY - EF.CtrPoint.y + EF.Top
                    spriteRightDummy = spriteLeftDummy + spriteWidth
                    spriteBottomDummy = spriteTopDummy + spriteHeight
                End If
            End If
        Next

        Dim Xoverlap As Boolean = spriteLeft <= spriteRightDummy And spriteLeftDummy <= spriteRight
        Dim Yoverlap As Boolean = spriteBottom >= spriteTopDummy And spriteBottomDummy >= spriteTop

        If Xoverlap And Yoverlap Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
