'==============================================================================
' 檔案名稱: Form1.vb
' 專案名稱: 114 工科賽 PC 監控系統
' 功能說明: 監控電腦 CPU 和 RAM 使用率，並透過藍芽傳送資料至 Arduino/MCU
'          支援 EEPROM 資料寫入、COM Port 熱插拔偵測等功能
' 競賽資訊: 113 學年度工業類科學生技藝競賽 - 電腦修護職種
' 開發單位: 新化高工
' 崗位編號: 01
' 最後更新: 2025-01-11
'==============================================================================

Imports System.IO.Ports
Imports System.Text.RegularExpressions
Imports System.Management

Public Class Form1
    '==========================================================================
    ' 常數定義區
    '==========================================================================
    Private Const STATION_NUMBER As Integer = 1
    Private Const BAUD_RATE As Integer = 9600

    ' 封包協定常數
    Private Const PACKET_SOF As Byte = &HFF
    Private Const PACKET_EOF As Byte = &HFE
    Private Const CMD_CPU As Byte = Asc("C"c)
    Private Const CMD_RAM As Byte = Asc("R"c)
    Private Const CMD_EEPROM As Byte = Asc("E"c)
    Private Const CMD_CONNECT As String = "c"
    
    ' 顏色閾值常數
    Private Const THRESHOLD_LOW As Integer = 50
    Private Const THRESHOLD_MEDIUM As Integer = 84

    '==========================================================================
    ' 類別層級變數
    '==========================================================================
    Private cpuCounter As PerformanceCounter
    Private ramCounter As PerformanceCounter
    Private WithEvents portWatcher As ManagementEventWatcher

    '==========================================================================
    ' 初始化
    '==========================================================================
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializeUI()
        InitializePerformanceCounters()
        InitializePortWatcher()
        UpdateConnectionStatus(False)
    End Sub

    Private Sub InitializeUI()
        Me.Text = $"113 學年度 工業類科學生技藝競賽 電腦修護職種 台中高工 第二站 崗位號碼：{STATION_NUMBER:D2}"
        SetBluetoothName()
        LoadComPorts()
    End Sub

    '==========================================================================
    ' 藍牙模組名稱
    '==========================================================================
    Private Sub SetBluetoothName()
        Dim binary As String = Convert.ToString(STATION_NUMBER, 2).PadLeft(8, "0"c)
        Dim last4Bits As String = binary.Substring(4)
        Dim prefix As String = If(STATION_NUMBER Mod 2 = 1, "ODD", "EVEN")
        LabelBluetoothName.Text = $"{prefix}-{STATION_NUMBER:D2}-{last4Bits}"
    End Sub

    '==========================================================================
    ' COM Port 管理
    '==========================================================================
    Private Sub LoadComPorts()
        ComboBoxCOM.Items.Clear()
        Dim ports As String() = SerialPort.GetPortNames()
        
        If ports.Length > 0 Then
            ComboBoxCOM.Items.AddRange(ports)
            ComboBoxCOM.SelectedIndex = 0
        Else
            ShowMessage("未偵測到任何 COM Port", "提示", MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub ButtonRefresh_Click(sender As Object, e As EventArgs) Handles ButtonRefresh.Click
        LoadComPorts()
    End Sub

    '==========================================================================
    ' 連線管理
    '==========================================================================
    Private Sub ButtonOpen_Click(sender As Object, e As EventArgs) Handles ButtonOpen.Click
        If ComboBoxCOM.SelectedItem Is Nothing Then
            ShowMessage("請選擇 COM Port", "錯誤", MessageBoxIcon.Warning)
            Return
        End If

        Try
            ConfigureSerialPort()
            SerialPort1.Open()
            SerialPort1.Write(CMD_CONNECT)
            
            LogDebug("COM Port 連線成功", SerialPort1.PortName, BAUD_RATE)
            UpdateConnectionStatus(True)
            ShowMessage("連線成功！現在可以啟動 CPU 和 RAM 監控。", "成功", MessageBoxIcon.Information)
        Catch ex As Exception
            HandleError("COM Port 連線失敗", ex)
        End Try
    End Sub

    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        If Not SerialPort1.IsOpen Then Return
        
        Try
            StopAllMonitoring()
            SerialPort1.Close()
            UpdateConnectionStatus(False)
            ResetDisplayValues()
            ShowMessage("已中斷連線", "提示", MessageBoxIcon.Information)
        Catch ex As Exception
            HandleError("關閉 COM Port 時發生錯誤", ex)
        End Try
    End Sub

    Private Sub ConfigureSerialPort()
        With SerialPort1
            .PortName = ComboBoxCOM.SelectedItem.ToString()
            .BaudRate = BAUD_RATE
            .DataBits = 8
            .Parity = Parity.None
            .StopBits = StopBits.One
        End With
    End Sub

    Private Sub UpdateConnectionStatus(isConnected As Boolean)
        With LabelConnectionStatus
            .Text = If(isConnected, "Connected", "Disconnect")
            .ForeColor = If(isConnected, Color.Green, Color.Red)
        End With
        
        ButtonOpen.Enabled = Not isConnected
        ButtonClose.Enabled = isConnected
        ButtonWrite.Enabled = isConnected
        ButtonStartCPU.Enabled = isConnected
        ButtonStartRAM.Enabled = isConnected
        
        If Not isConnected Then StopAllMonitoring()
    End Sub

    Private Sub StopAllMonitoring()
    End Sub

    '==========================================================================
    ' 效能計數器
    '==========================================================================
    Private Sub InitializePerformanceCounters()
        Try
            cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")
            ramCounter = New PerformanceCounter("Memory", "% Committed Bytes In Use")
            cpuCounter.NextValue()
            ramCounter.NextValue()
        Catch ex As Exception
            HandleError("無法初始化效能計數器", ex)
        End Try
    End Sub

    '==========================================================================
    ' CPU 監控
    '==========================================================================
    Private Sub ButtonStartCPU_Click(sender As Object, e As EventArgs) Handles ButtonStartCPU.Click
        ToggleMonitoring(TimerCPU, ButtonStartCPU, ButtonStopCPU, True)
    End Sub

    Private Sub ButtonStopCPU_Click(sender As Object, e As EventArgs) Handles ButtonStopCPU.Click
        ToggleMonitoring(TimerCPU, ButtonStartCPU, ButtonStopCPU, False)
        PanelCPUColor.BackColor = Color.Gray
    End Sub

    ''' <summary>
    ''' CPU 監控計時器事件（每秒觸發一次）
    ''' 讀取 CPU 使用率並更新顯示
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 從效能計數器讀取 CPU 使用率
    ''' 2. 四捨五入為整數百分比
    ''' 3. 更新標籤顯示（例如: "65 %"）
    ''' 4. 根據使用率判斷顏色（綠/黃/紅）
    ''' 5. 更新顏色方塊背景色
    ''' 6. 若已連線，則傳送 "LOAD " + 數值 + 換行符號至 Arduino
    '''    例如: "LOAD 30\n", "LOAD 65\n", "LOAD 90\n"
    ''' </remarks>
    Private Sub TimerCPU_Tick(sender As Object, e As EventArgs) Handles TimerCPU.Tick
        Try
            ' 從效能計數器讀取 CPU 使用率
            ' 回傳值為 Single 型別，範圍 0.0 ~ 100.0
            Dim cpuUsage As Single = cpuCounter.NextValue()

            ' 四捨五入為整數（例如: 64.7 → 65）
            Dim cpuPercent As Integer = CInt(Math.Round(cpuUsage))

            ' 更新 UI 標籤顯示 CPU 使用率
            ' 使用字串插值格式化（例如: "65 %"）
            LabelCPUValue.Text = $"{cpuPercent} %"

            ' 根據使用率百分比判斷對應的顏色
            ' 0-50%: 綠色, 51-84%: 黃色, ≥85%: 紅色
            Dim color As Color = GetLoadingColor(cpuPercent)

            ' 更新顏色方塊的背景色
            PanelCPUColor.BackColor = color

            ' 若序列埠處於開啟狀態（已連線 Arduino）
            ' 傳送 "LOAD " + CPU 使用率 + 換行符號
            ' 換行符號用於 Arduino 端使用 Serial.readStringUntil('\n') 解析
            ' 例如: "LOAD 30\n", "LOAD 65\n", "LOAD 90\n"
            If SerialPort1.IsOpen Then
                SerialPort1.Write($"LOAD {cpuPercent}" & vbLf)
            End If

        Catch ex As Exception
            ' 捕捉監控過程中的例外
            ' 可能原因: 效能計數器失效、權限問題
            MessageBox.Show($"CPU 監控錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' RAM 監控
    '==========================================================================
    Private Sub ButtonStartRAM_Click(sender As Object, e As EventArgs) Handles ButtonStartRAM.Click
        ToggleMonitoring(TimerRAM, ButtonStartRAM, ButtonStopRAM, True)
    End Sub

    Private Sub ButtonStopRAM_Click(sender As Object, e As EventArgs) Handles ButtonStopRAM.Click
        ToggleMonitoring(TimerRAM, ButtonStartRAM, ButtonStopRAM, False)
        PanelRAMColor.BackColor = Color.Gray
    End Sub

    ''' <summary>
    ''' RAM 監控計時器事件（每秒觸發一次）
    ''' 讀取 RAM 使用率並更新顯示
    ''' </summary>
    ''' <remarks>
    ''' 執行流程與 CPU 監控相同，但讀取的是 RAM 使用率
    ''' 傳送格式: "LOAD " + 數值 + 換行符號
    ''' 例如: "LOAD 20\n", "LOAD 55\n", "LOAD 88\n"
    ''' </remarks>
    Private Sub TimerRAM_Tick(sender As Object, e As EventArgs) Handles TimerRAM.Tick
        Try
            ' 呼叫 GetRAMUsage() 函數取得 RAM 使用率
            Dim ramUsage As Single = GetRAMUsage()

            ' 四捨五入為整數
            Dim ramPercent As Integer = CInt(Math.Round(ramUsage))

            ' 更新 UI 標籤顯示 RAM 使用率
            LabelRAMValue.Text = $"{ramPercent} %"

            ' 根據使用率判斷對應的顏色
            Dim color As Color = GetLoadingColor(ramPercent)

            ' 更新顏色方塊的背景色
            PanelRAMColor.BackColor = color

            ' 若已連線，則傳送 "LOAD " + RAM 使用率 + 換行符號
            ' 換行符號用於 Arduino 端使用 Serial.readStringUntil('\n') 解析
            ' 例如: "LOAD 20\n", "LOAD 55\n", "LOAD 88\n"
            If SerialPort1.IsOpen Then
                SerialPort1.Write($"LOAD {ramPercent}" & vbLf)
            End If

        Catch ex As Exception
            ' 捕捉監控過程中的例外
            MessageBox.Show($"RAM 監控錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' 取得系統 RAM 使用率
    ''' </summary>
    ''' <returns>RAM 使用率百分比（0-100）</returns>
    ''' <remarks>
    ''' 使用 Windows Performance Counter 讀取記憶體使用率
    ''' 若計數器未初始化或發生錯誤，則回傳 0
    ''' </remarks>
    Private Function GetRAMUsage() As Single
        Try
            If ramCounter IsNot Nothing Then
                Return ramCounter.NextValue()
            Else
                Return 0
            End If
        Catch ex As Exception
            Return 0
        End Try
    End Function

    '==========================================================================
    ' 監控共用邏輯
    '==========================================================================
    Private Sub ToggleMonitoring(timer As Timer, startBtn As Button, stopBtn As Button, isStart As Boolean)
        If isStart Then
            timer.Start()
            startBtn.Enabled = False
            stopBtn.Enabled = True
        Else
            timer.Stop()
            startBtn.Enabled = True
            stopBtn.Enabled = False
        End If
    End Sub

    Private Function GetLoadingColor(percent As Integer) As Color
        Return If(percent <= THRESHOLD_LOW, Color.Green,
               If(percent <= THRESHOLD_MEDIUM, Color.Yellow, Color.Red))
    End Function

    '==========================================================================
    ' EEPROM 寫入
    '==========================================================================
    Private Sub TextBoxBinary_TextChanged(sender As Object, e As EventArgs) Handles TextBoxBinary.TextChanged
        Dim input As String = TextBoxBinary.Text.Trim()

        If String.IsNullOrEmpty(input) Then
            SetDecimalValue("0", Color.Blue)
            Return
        End If

        If Regex.IsMatch(input, "^[01]+$") Then
            Try
                Dim decimalValue As Integer = Convert.ToInt32(input, 2)
                SetDecimalValue(decimalValue.ToString(), Color.Blue)
            Catch
                SetDecimalValue("溢位", Color.Red)
            End Try
        Else
            SetDecimalValue("格式錯誤", Color.Red)
        End If
    End Sub

    Private Sub ButtonWrite_Click(sender As Object, e As EventArgs) Handles ButtonWrite.Click
        Dim input As String = TextBoxBinary.Text.Trim()

        If Not Regex.IsMatch(input, "^[01]{4}$") Then
            ShowMessage("Not BIN Format", "格式錯誤", MessageBoxIcon.Warning)
            TextBoxBinary.Clear()
            SetDecimalValue("0", Color.Blue)
            Return
        End If

        Try
            Dim decimalValue As Integer = Convert.ToInt32(input, 2)
            Dim packet As New List(Of Byte) From {
                PACKET_SOF,
                CMD_EEPROM,
                1,
                CByte(decimalValue),
                CByte(decimalValue And &HFF),
                PACKET_EOF
            }

            If SerialPort1.IsOpen Then
                SerialPort1.Write(packet.ToArray(), 0, packet.Count)
                ShowMessage($"已寫入 EEPROM：二進位 {input} → 十進位 {decimalValue}",
                          "成功", MessageBoxIcon.Information)
            Else
                ShowMessage("未連線，無法寫入 EEPROM", "錯誤", MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            HandleError("寫入失敗", ex)
        End Try
    End Sub

    '==========================================================================
    ' COM Port 熱插拔監控
    '==========================================================================
    Private Sub InitializePortWatcher()
        Try
            Dim query As New WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3")
            portWatcher = New ManagementEventWatcher(query)
            AddHandler portWatcher.EventArrived, AddressOf OnPortChanged
            portWatcher.Start()
        Catch ex As Exception
            ' 靜默處理，不影響主要功能
        End Try
    End Sub

    Private Sub OnPortChanged(sender As Object, e As EventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(AddressOf LoadComPorts))
        Else
            LoadComPorts()
        End If
    End Sub

    '==========================================================================
    ' 程式結束
    '==========================================================================
    Private Sub ButtonExit_Click(sender As Object, e As EventArgs) Handles ButtonExit.Click
        CleanupResources()
        Application.Exit()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        CleanupResources()
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub CleanupResources()
        If SerialPort1?.IsOpen Then SerialPort1.Close()
        cpuCounter?.Dispose()
        ramCounter?.Dispose()
        If portWatcher IsNot Nothing Then
            portWatcher.Stop()
            portWatcher.Dispose()
        End If
    End Sub

    '==========================================================================
    ' 輔助方法
    '==========================================================================
    Private Sub ResetDisplayValues()
        LabelCPUValue.Text = "0 %"
        LabelRAMValue.Text = "0 %"
        PanelCPUColor.BackColor = Color.Gray
        PanelRAMColor.BackColor = Color.Gray
    End Sub

    Private Sub SetDecimalValue(text As String, color As Color)
        LabelDecimalValue.Text = text
        LabelDecimalValue.ForeColor = color
    End Sub

    Private Sub ShowMessage(message As String, title As String, icon As MessageBoxIcon)
        MessageBox.Show(message, title, MessageBoxButtons.OK, icon)
    End Sub

    Private Sub HandleError(context As String, ex As Exception)
        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {context}：{ex.Message}")
        ShowMessage($"{context}：{ex.Message}", "錯誤", MessageBoxIcon.Error)
    End Sub

    Private Sub LogDebug(message As String, portName As String, baudRate As Integer)
        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}")
        Debug.WriteLine($"  - Port: {portName}")
        Debug.WriteLine($"  - Baud Rate: {baudRate}")
        Debug.WriteLine($"  - 連線確認字元 'c' 已發送")
    End Sub

End Class

'==============================================================================
' 程式碼結束
'==============================================================================
