'==============================================================================
' 檔案名稱: Form1.vb
' 專案名稱: 114 工科賽 PC 監控系統
' 功能說明: 監控電腦 CPU 和 RAM 使用率，並透過藍芽傳送資料至 Arduino/MCU
'          支援 EEPROM 資料寫入、COM Port 熱插拔偵測等功能
' 競賽資訊: 113 學年度工業類科學生技藝競賽 - 電腦修護職種
' 開發單位: 新化高工
' 崗位編號: 01
' 最後更新: 2025-01-XX
'==============================================================================

Imports System.IO.Ports                ' 序列埠通訊相關類別
Imports System.Text.RegularExpressions ' 正規表達式驗證
Imports System.Management               ' WMI 系統管理（用於 COM Port 熱插拔偵測）

Public Class Form1
    '==========================================================================
    ' 常數定義區
    '==========================================================================
    ''' <summary>
    ''' 崗位編號，用於產生藍牙模組名稱
    ''' 範例: 1 → ODD-01-0001, 2 → EVEN-02-0010
    ''' </summary>
    Private Const STATION_NUMBER As Integer = 1

    ''' <summary>
    ''' 序列埠通訊速率（Baud Rate）
    ''' Arduino 端也必須設定為相同數值
    ''' </summary>
    Private Const BAUD_RATE As Integer = 9600

    '==========================================================================
    ' 類別層級變數宣告區
    '==========================================================================
    ''' <summary>
    ''' CPU 使用率效能計數器
    ''' 用於讀取系統 CPU 使用率百分比
    ''' </summary>
    Private cpuCounter As PerformanceCounter

    ''' <summary>
    ''' RAM 使用率效能計數器
    ''' 用於讀取系統記憶體使用率百分比
    ''' </summary>
    Private ramCounter As PerformanceCounter

    ''' <summary>
    ''' COM Port 熱插拔監控器
    ''' 使用 WMI 事件監聽 USB 裝置的插入與移除
    ''' WithEvents 關鍵字允許處理此物件的事件
    ''' </summary>
    Private WithEvents portWatcher As ManagementEventWatcher

    '==========================================================================
    ' 表單載入事件
    '==========================================================================
    ''' <summary>
    ''' 表單載入事件處理程序
    ''' 在程式啟動時執行初始化工作
    ''' </summary>
    ''' <remarks>
    ''' 執行順序:
    ''' 1. 設定視窗標題（P1）
    ''' 2. 計算並顯示藍牙模組名稱（P2）
    ''' 3. 載入可用的 COM Port 清單（P3）
    ''' 4. 初始化 CPU 和 RAM 效能計數器
    ''' 5. 啟動 COM Port 熱插拔監控
    ''' 6. 設定初始 UI 狀態
    ''' 7. 啟用 CPU 和 RAM 監控按鈕（獨立於連線狀態）
    ''' </remarks>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' P1: 設定視窗標題列文字
        ' 顯示競賽資訊、學校、站別和崗位編號
        Me.Text = "113 學年度 工業類科學生技藝競賽 電腦修護職種 台中高工 第二站 崗位號碼：01"

        ' P2: 根據崗位編號自動產生並顯示藍牙模組名稱
        ' 格式: ODD/EVEN-編號-二進位後4位
        SetBluetoothName()

        ' P3: 掃描並載入系統中所有可用的 COM Port
        ' 結果會顯示在下拉選單中供使用者選擇
        LoadComPorts()

        ' 初始化 Windows 效能計數器
        ' 用於讀取 CPU 和 RAM 使用率
        InitializePerformanceCounters()

        ' 啟動 COM Port 熱插拔監控功能
        ' 當 USB 裝置插入或移除時會自動更新 COM Port 清單
        InitializePortWatcher()

        ' 設定初始連線狀態為「未連線」
        ' 會更新 UI 元件的啟用/停用狀態
        UpdateConnectionStatus(False)

        ' 啟用 CPU 和 RAM 監控按鈕
        ' 即使未連線藍芽也可以執行本地監控
        ButtonStartCPU.Enabled = True
        ButtonStartRAM.Enabled = True
    End Sub

    '==========================================================================
    ' P2: 藍牙模組名稱邏輯
    '==========================================================================
    ''' <summary>
    ''' 根據崗位編號自動產生藍牙模組名稱
    ''' </summary>
    ''' <remarks>
    ''' 命名規則:
    ''' 1. 將崗位編號轉換為 8 位元二進位
    ''' 2. 取二進位的後 4 位
    ''' 3. 判斷編號奇偶性
    ''' 4. 格式化為 ODD/EVEN-編號-後4位
    ''' 
    ''' 範例:
    ''' - 崗位 1: 00000001 → 0001 → 奇數 → ODD-01-0001
    ''' - 崗位 2: 00000010 → 0010 → 偶數 → EVEN-02-0010
    ''' - 崗位 10: 00001010 → 1010 → 偶數 → EVEN-10-1010
    ''' </remarks>
    Private Sub SetBluetoothName()
        ' 將崗位編號轉換為 8 位元二進位字串
        ' 例如: 1 → "00000001", 10 → "00001010"
        Dim binary As String = Convert.ToString(STATION_NUMBER, 2).PadLeft(8, "0"c)

        ' 取二進位字串的後 4 位（索引 4 到 7）
        ' 例如: "00000001" → "0001"
        Dim last4Bits As String = binary.Substring(4)

        ' 判斷崗位編號是否為奇數
        ' 使用模除運算: 奇數 Mod 2 = 1, 偶數 Mod 2 = 0
        Dim isOdd As Boolean = (STATION_NUMBER Mod 2 = 1)

        ' 根據奇偶性選擇前綴字並格式化顯示
        If isOdd Then
            ' 奇數: ODD-編號(2位數)-二進位後4位
            LabelBluetoothName.Text = $"ODD-{STATION_NUMBER:D2}-{last4Bits}"
        Else
            ' 偶數: EVEN-編號(2位數)-二進位後4位
            LabelBluetoothName.Text = $"EVEN-{STATION_NUMBER:D2}-{last4Bits}"
        End If
    End Sub

    '==========================================================================
    ' P3: COM Port 管理
    '==========================================================================
    ''' <summary>
    ''' 載入系統中所有可用的 COM Port 清單
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 清空下拉選單中的舊資料
    ''' 2. 呼叫系統 API 取得所有可用 COM Port
    ''' 3. 將 COM Port 加入下拉選單
    ''' 4. 自動選擇第一個項目
    ''' 5. 若無可用 COM Port 則顯示提示訊息
    ''' </remarks>
    Private Sub LoadComPorts()
        ' 清空 ComboBox 中的所有項目，準備重新載入
        ComboBoxCOM.Items.Clear()

        ' 呼叫 SerialPort.GetPortNames() 取得系統中所有可用的 COM Port
        ' 回傳值為字串陣列，例如: ["COM1", "COM3", "COM5"]
        Dim ports As String() = SerialPort.GetPortNames()

        ' 檢查是否有可用的 COM Port
        If ports.Length > 0 Then
            ' 將所有 COM Port 名稱加入下拉選單
            ComboBoxCOM.Items.AddRange(ports)

            ' 自動選擇第一個項目（索引 0）
            ' 方便使用者直接點擊連線
            If ComboBoxCOM.Items.Count > 0 Then
                ComboBoxCOM.SelectedIndex = 0
            End If
        Else
            ' 若系統中沒有任何 COM Port，顯示提示訊息
            ' 可能原因: 未插入 USB 轉 Serial 裝置、藍芽未配對等
            MessageBox.Show("未偵測到任何 COM Port", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    ''' <summary>
    ''' 重新整理按鈕點擊事件
    ''' 重新掃描並載入可用的 COM Port
    ''' </summary>
    ''' <remarks>
    ''' 使用情境:
    ''' - 插入新的 USB 轉 Serial 裝置後
    ''' - 完成藍芽配對後
    ''' - COM Port 清單顯示不正確時
    ''' </remarks>
    Private Sub ButtonRefresh_Click(sender As Object, e As EventArgs) Handles ButtonRefresh.Click
        ' 重新載入 COM Port 清單
        LoadComPorts()
    End Sub

    '==========================================================================
    ' P4: COM Port 連線管理
    '==========================================================================
    ''' <summary>
    ''' 開啟 COM Port 按鈕點擊事件
    ''' 建立與選定 COM Port 的序列埠連線
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 驗證是否已選擇 COM Port
    ''' 2. 設定序列埠參數（Baud Rate, Data Bits, Parity, Stop Bits）
    ''' 3. 開啟序列埠連線
    ''' 4. 發送連線確認字元 'c' 至 Arduino
    ''' 5. 更新 UI 連線狀態
    ''' 6. 顯示連線成功訊息
    ''' 
    ''' 序列埠參數說明:
    ''' - BaudRate: 9600 (通訊速率)
    ''' - DataBits: 8 (資料位元數)
    ''' - Parity: None (無同位檢查)
    ''' - StopBits: One (1 個停止位元)
    ''' </remarks>
    Private Sub ButtonOpen_Click(sender As Object, e As EventArgs) Handles ButtonOpen.Click
        ' 檢查使用者是否已從下拉選單選擇 COM Port
        If ComboBoxCOM.SelectedItem Is Nothing Then
            ' 若未選擇，顯示警告訊息並結束程序
            MessageBox.Show("請選擇 COM Port", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' 使用 With 語句簡化 SerialPort1 的屬性設定
            With SerialPort1
                ' 設定要開啟的 COM Port 名稱（例如: "COM3"）
                .PortName = ComboBoxCOM.SelectedItem.ToString()

                ' 設定 Baud Rate (通訊速率)
                ' Arduino 端也必須使用相同的 Baud Rate
                .BaudRate = BAUD_RATE

                ' 設定資料位元數（通常為 8）
                .DataBits = 8

                ' 設定同位檢查方式（None = 無同位檢查）
                .Parity = Parity.None

                ' 設定停止位元數（One = 1 個停止位元）
                .StopBits = StopBits.One

                ' 開啟序列埠連線
                ' 若 COM Port 被其他程式佔用或不存在，會拋出例外
                .Open()
            End With

            ' P4 需求: 連線成功後發送字元 'c' 至 Arduino
            ' 作為連線確認訊號，Arduino 端可據此判斷 PC 已連線
            SerialPort1.Write("c")

            ' 更新 UI 狀態為「已連線」
            ' 會啟用 Close 按鈕和 Write 按鈕
            UpdateConnectionStatus(True)

            ' 顯示連線成功的提示訊息
            MessageBox.Show("連線成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            ' 捕捉任何連線過程中發生的例外
            ' 常見錯誤: COM Port 不存在、存取被拒、已被佔用
            MessageBox.Show($"無法開啟 COM Port：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' P5: 關閉 COM Port 連線
    '==========================================================================
    ''' <summary>
    ''' 關閉 COM Port 按鈕點擊事件
    ''' 中斷與 COM Port 的序列埠連線
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 檢查序列埠是否處於開啟狀態
    ''' 2. 停止所有正在執行的監控計時器
    ''' 3. 關閉序列埠連線
    ''' 4. 更新 UI 連線狀態為「未連線」
    ''' 5. 重置所有顯示數值為初始狀態
    ''' 6. 顯示中斷連線訊息
    ''' </remarks>
    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        Try
            ' 檢查序列埠是否處於開啟狀態
            If SerialPort1.IsOpen Then
                ' 停止 CPU 監控計時器
                ' 避免在關閉連線後仍嘗試傳送資料
                TimerCPU.Stop()

                ' 停止 RAM 監控計時器
                TimerRAM.Stop()

                ' 關閉序列埠連線
                ' 釋放 COM Port 資源，允許其他程式使用
                SerialPort1.Close()

                ' 更新 UI 狀態為「未連線」
                ' 會停用 Close 按鈕和 Write 按鈕
                UpdateConnectionStatus(False)

                ' 重置所有監控顯示數值為 0 %
                ' 並將顏色方塊設回灰色
                ResetDisplayValues()

                ' 顯示中斷連線的提示訊息
                MessageBox.Show("已中斷連線", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            ' 捕捉關閉過程中發生的例外
            ' 雖然不常見，但仍需處理以確保程式穩定
            MessageBox.Show($"關閉 COM Port 時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' P6: 連線狀態管理
    '==========================================================================
    ''' <summary>
    ''' 更新連線狀態並調整 UI 元件的啟用/停用狀態
    ''' </summary>
    ''' <param name="isConnected">True: 已連線, False: 未連線</param>
    ''' <remarks>
    ''' 連線時:
    ''' - 連線狀態標籤顯示綠色 "Connected"
    ''' - 停用 Open 按鈕（避免重複開啟）
    ''' - 啟用 Close 按鈕
    ''' - 啟用 Write to EEPROM 按鈕（需要連線才能寫入）
    ''' 
    ''' 未連線時:
    ''' - 連線狀態標籤顯示紅色 "Disconnect"
    ''' - 啟用 Open 按鈕
    ''' - 停用 Close 按鈕
    ''' - 停用 Write to EEPROM 按鈕
    ''' 
    ''' 注意: CPU 和 RAM 監控按鈕不受連線狀態影響，可獨立運作
    ''' </remarks>
    Private Sub UpdateConnectionStatus(isConnected As Boolean)
        If isConnected Then
            ' ===== 已連線狀態 =====

            ' 更新連線狀態標籤文字和顏色
            LabelConnectionStatus.Text = "Connected"
            LabelConnectionStatus.ForeColor = Color.Green

            ' 調整按鈕啟用狀態
            ButtonOpen.Enabled = False   ' 停用 Open 按鈕（已連線）
            ButtonClose.Enabled = True   ' 啟用 Close 按鈕
            ButtonWrite.Enabled = True   ' 啟用 EEPROM 寫入按鈕
        Else
            ' ===== 未連線狀態 =====

            ' 更新連線狀態標籤文字和顏色
            LabelConnectionStatus.Text = "Disconnect"
            LabelConnectionStatus.ForeColor = Color.Red

            ' 調整按鈕啟用狀態
            ButtonOpen.Enabled = True    ' 啟用 Open 按鈕
            ButtonClose.Enabled = False  ' 停用 Close 按鈕（未連線）
            ButtonWrite.Enabled = False  ' 停用 EEPROM 寫入按鈕（需要連線）
        End If
    End Sub

    '==========================================================================
    ' 效能計數器初始化
    '==========================================================================
    ''' <summary>
    ''' 初始化 Windows 效能計數器
    ''' 用於讀取 CPU 和 RAM 使用率
    ''' </summary>
    ''' <remarks>
    ''' CPU 計數器:
    ''' - Category: "Processor"
    ''' - Counter: "% Processor Time"
    ''' - Instance: "_Total" (所有 CPU 核心的平均值)
    ''' 
    ''' RAM 計數器:
    ''' - Category: "Memory"
    ''' - Counter: "% Committed Bytes In Use"
    ''' - 顯示已使用的記憶體百分比
    ''' 
    ''' 注意事項:
    ''' - 第一次呼叫 NextValue() 通常回傳 0
    ''' - 需要在初始化時先呼叫一次以啟動計數器
    ''' - 需要系統管理員權限才能存取某些效能計數器
    ''' </remarks>
    Private Sub InitializePerformanceCounters()
        Try
            ' ===== 初始化 CPU 效能計數器 =====

            ' 建立 CPU 使用率計數器
            ' "Processor": 效能類別
            ' "% Processor Time": 計數器名稱（CPU 使用率百分比）
            ' "_Total": 實例名稱（所有 CPU 核心的總和）
            cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")

            ' 初始化讀取（第一次呼叫通常回傳 0）
            ' 這樣可以啟動計數器，之後的讀取才會準確
            cpuCounter.NextValue()

            ' ===== 初始化 RAM 效能計數器 =====

            ' 建立記憶體使用率計數器
            ' "Memory": 效能類別
            ' "% Committed Bytes In Use": 計數器名稱（已使用記憶體百分比）
            ramCounter = New PerformanceCounter("Memory", "% Committed Bytes In Use")

            ' 初始化讀取
            ramCounter.NextValue()

        Catch ex As Exception
            ' 捕捉初始化失敗的例外
            ' 可能原因: 權限不足、效能計數器服務未啟動
            MessageBox.Show($"無法初始化效能計數器：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' P7: CPU 監控功能
    '==========================================================================
    ''' <summary>
    ''' 啟動 CPU 監控按鈕點擊事件
    ''' 開始定時讀取 CPU 使用率
    ''' </summary>
    ''' <remarks>
    ''' - 啟動計時器，每秒觸發一次 TimerCPU_Tick 事件
    ''' - 停用「Start CPU」按鈕，避免重複啟動
    ''' - 啟用「Stop CPU」按鈕，允許停止監控
    ''' </remarks>
    Private Sub ButtonStartCPU_Click(sender As Object, e As EventArgs) Handles ButtonStartCPU.Click
        ' 啟動 CPU 監控計時器（間隔 1000ms = 1秒）
        TimerCPU.Start()

        ' 調整按鈕狀態
        ButtonStartCPU.Enabled = False  ' 停用 Start 按鈕
        ButtonStopCPU.Enabled = True    ' 啟用 Stop 按鈕
    End Sub

    ''' <summary>
    ''' 停止 CPU 監控按鈕點擊事件
    ''' 停止讀取 CPU 使用率
    ''' </summary>
    ''' <remarks>
    ''' - 停止計時器
    ''' - 恢復按鈕狀態
    ''' - 將顏色方塊重置為灰色
    ''' </remarks>
    Private Sub ButtonStopCPU_Click(sender As Object, e As EventArgs) Handles ButtonStopCPU.Click
        ' 停止 CPU 監控計時器
        TimerCPU.Stop()

        ' 調整按鈕狀態
        ButtonStartCPU.Enabled = True   ' 啟用 Start 按鈕
        ButtonStopCPU.Enabled = False   ' 停用 Stop 按鈕

        ' 將 CPU 顏色方塊重置為灰色（預設狀態）
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
    ''' 6. 若已連線，則傳送資料至 Arduino
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
            ' 則將 CPU 資料封包傳送至 MCU
            If SerialPort1.IsOpen Then
                SendLoadingData("CPU", cpuPercent, color)
            End If

        Catch ex As Exception
            ' 捕捉監控過程中的例外
            ' 可能原因: 效能計數器失效、權限問題
            MessageBox.Show($"CPU 監控錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' RAM 監控功能
    '==========================================================================
    ''' <summary>
    ''' 啟動 RAM 監控按鈕點擊事件
    ''' 開始定時讀取 RAM 使用率
    ''' </summary>
    Private Sub ButtonStartRAM_Click(sender As Object, e As EventArgs) Handles ButtonStartRAM.Click
        ' 啟動 RAM 監控計時器（間隔 1000ms = 1秒）
        TimerRAM.Start()

        ' 調整按鈕狀態
        ButtonStartRAM.Enabled = False  ' 停用 Start 按鈕
        ButtonStopRAM.Enabled = True    ' 啟用 Stop 按鈕
    End Sub

    ''' <summary>
    ''' 停止 RAM 監控按鈕點擊事件
    ''' 停止讀取 RAM 使用率
    ''' </summary>
    Private Sub ButtonStopRAM_Click(sender As Object, e As EventArgs) Handles ButtonStopRAM.Click
        ' 停止 RAM 監控計時器
        TimerRAM.Stop()

        ' 調整按鈕狀態
        ButtonStartRAM.Enabled = True   ' 啟用 Start 按鈕
        ButtonStopRAM.Enabled = False   ' 停用 Stop 按鈕

        ' 將 RAM 顏色方塊重置為灰色（預設狀態）
        PanelRAMColor.BackColor = Color.Gray
    End Sub

    ''' <summary>
    ''' RAM 監控計時器事件（每秒觸發一次）
    ''' 讀取 RAM 使用率並更新顯示
    ''' </summary>
    ''' <remarks>
    ''' 執行流程與 CPU 監控相同，但讀取的是 RAM 使用率
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

            ' 若已連線，則傳送 RAM 資料封包至 Arduino
            If SerialPort1.IsOpen Then
                SendLoadingData("RAM", ramPercent, color)
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
            ' 檢查 RAM 計數器是否已初始化
            If ramCounter IsNot Nothing Then
                ' 讀取並回傳 RAM 使用率（0.0 ~ 100.0）
                Return ramCounter.NextValue()
            Else
                ' 計數器未初始化，回傳 0
                Return 0
            End If
        Catch ex As Exception
            ' 發生錯誤時回傳 0，採用靜默處理避免中斷監控
            Return 0
        End Try
    End Function

    '==========================================================================
    ' P7: 顏色判斷邏輯
    '==========================================================================
    ''' <summary>
    ''' 根據使用率百分比判斷對應的顏色
    ''' </summary>
    ''' <param name="percent">使用率百分比（0-100）</param>
    ''' <returns>對應的顏色（綠/黃/紅）</returns>
    ''' <remarks>
    ''' 顏色對應規則:
    ''' - 0% ~ 50%: 綠色（正常負載）
    ''' - 51% ~ 84%: 黃色（中度負載）
    ''' - 85% ~ 100%: 紅色（高負載）
    ''' 
    ''' 此規則同時適用於 CPU 和 RAM 監控
    ''' </remarks>
    Private Function GetLoadingColor(percent As Integer) As Color
        If percent <= 50 Then
            ' 低負載: 0-50%，顯示綠色
            Return Color.Green
        ElseIf percent <= 84 Then
            ' 中度負載: 51-84%，顯示黃色
            Return Color.Yellow
        Else
            ' 高負載: 85% 以上，顯示紅色
            Return Color.Red
        End If
    End Function

    '==========================================================================
    ' 資料傳送功能
    '==========================================================================
    ''' <summary>
    ''' 將 CPU 或 RAM 監控資料封包傳送至 Arduino/MCU
    ''' </summary>
    ''' <param name="type">資料類型（"CPU" 或 "RAM"）</param>
    ''' <param name="percent">使用率百分比（0-100）</param>
    ''' <param name="color">對應的顏色</param>
    ''' <remarks>
    ''' 封包格式: [SOF][CMD][LEN][Data][CHK][EOF]
    ''' 
    ''' 欄位說明:
    ''' - SOF (Start of Frame): 0xFF（封包起始標記）
    ''' - CMD (Command): 'C' (0x43) = CPU, 'R' (0x52) = RAM
    ''' - LEN (Length): 0x04（資料長度 = 4 bytes）
    ''' - Data: [使用率%, R, G, B]（4 bytes）
    ''' - CHK (Checksum): (% + R + G + B) & 0xFF
    ''' - EOF (End of Frame): 0xFE（封包結束標記）
    ''' 
    ''' 範例（CPU 65%, Yellow RGB(255,255,0)):
    ''' 0xFF 0x43 0x04 0x41 0xFF 0xFF 0x00 0x3F 0xFE
    '''  ↑    ↑    ↑    ↑    ↑    ↑    ↑    ↑    ↑
    ''' SOF  CMD  LEN   %    R    G    B   CHK  EOF
    ''' </remarks>
    Private Sub SendLoadingData(type As String, percent As Integer, color As Color)
        Try
            ' 建立封包資料緩衝區（使用 List(Of Byte) 方便動態新增）
            Dim packet As New List(Of Byte)

            ' [SOF] 封包起始標記
            packet.Add(&HFF)  ' 0xFF = 255

            ' [CMD] 命令碼（區分 CPU 或 RAM）
            If type = "CPU" Then
                ' CPU 資料: 命令碼 = 'C' (ASCII 67 = 0x43)
                packet.Add(Asc("C"c))
            Else
                ' RAM 資料: 命令碼 = 'R' (ASCII 82 = 0x52)
                packet.Add(Asc("R"c))
            End If

            ' [LEN] 資料長度
            packet.Add(4)  ' 固定 4 bytes (percent + RGB)

            ' [Data] 實際資料（4 bytes）
            packet.Add(CByte(percent))  ' Byte 1: 使用率百分比 (0-100)
            packet.Add(color.R)         ' Byte 2: 紅色分量 (0-255)
            packet.Add(color.G)         ' Byte 3: 綠色分量 (0-255)
            packet.Add(color.B)         ' Byte 4: 藍色分量 (0-255)

            ' [CHK] 計算檢查碼（Checksum）
            ' 將所有 Data 欄位相加，並與 0xFF 進行 AND 運算
            ' 這樣可以確保結果在 0-255 範圍內
            Dim checksum As Byte = CByte((percent + color.R + color.G + color.B) And &HFF)
            packet.Add(checksum)

            ' [EOF] 封包結束標記
            packet.Add(&HFE)  ' 0xFE = 254

            ' 將封包陣列傳送至序列埠
            ' ToArray() 將 List(Of Byte) 轉換為 Byte()
            SerialPort1.Write(packet.ToArray(), 0, packet.Count)

        Catch ex As Exception
            ' 傳送失敗時採用靜默處理
            ' 不顯示錯誤訊息，避免中斷監控流程
            ' 常見原因: 連線中斷、Arduino 未回應
        End Try
    End Sub

    '==========================================================================
    ' P8: 二進位輸入驗證與即時轉換
    '==========================================================================
    ''' <summary>
    ''' 二進位輸入框文字變更事件
    ''' 即時驗證輸入格式並轉換為十進位顯示
    ''' </summary>
    ''' <remarks>
    ''' 驗證規則:
    ''' - 僅接受 '0' 和 '1' 字元
    ''' - 其他字元顯示「格式錯誤」
    ''' - 空白時顯示 "0"
    ''' - 超出範圍時顯示「溢位」
    ''' 
    ''' 正常輸入範例:
    ''' - "1010" → "10"（藍色）
    ''' - "1111" → "15"（藍色）
    ''' 
    ''' 錯誤輸入範例:
    ''' - "10a0" → "格式錯誤"（紅色）
    ''' - "" → "0"（藍色）
    ''' </remarks>
    Private Sub TextBoxBinary_TextChanged(sender As Object, e As EventArgs) Handles TextBoxBinary.TextChanged
        ' 取得輸入框文字並去除前後空白
        Dim input As String = TextBoxBinary.Text.Trim()

        ' 檢查輸入是否為空
        If String.IsNullOrEmpty(input) Then
            ' 空白時顯示 0
            LabelDecimalValue.Text = "0"
            Return
        End If

        ' 使用正規表達式驗證是否為有效的二進位格式
        ' ^[01]+$ 表示: 開頭到結尾只能包含 0 或 1，至少一個字元
        If Regex.IsMatch(input, "^[01]+$") Then
            ' ===== 格式正確，進行二進位轉十進位轉換 =====
            Try
                ' Convert.ToInt32(字串, 2) 將二進位字串轉為十進位整數
                ' 參數 2 表示來源是二進位（Base 2）
                Dim decimalValue As Integer = Convert.ToInt32(input, 2)

                ' 顯示轉換後的十進位值
                LabelDecimalValue.Text = decimalValue.ToString()

                ' 設定文字顏色為藍色（表示轉換成功）
                LabelDecimalValue.ForeColor = Color.Blue
            Catch
                ' 轉換失敗（可能是數值太大溢位）
                LabelDecimalValue.Text = "溢位"
                LabelDecimalValue.ForeColor = Color.Red
            End Try
        Else
            ' ===== 格式錯誤（包含非 0/1 字元）=====
            LabelDecimalValue.Text = "格式錯誤"
            LabelDecimalValue.ForeColor = Color.Red
        End If
    End Sub

    '==========================================================================
    ' P9: EEPROM 寫入功能
    '==========================================================================
    ''' <summary>
    ''' Write to EEPROM 按鈕點擊事件
    ''' 驗證二進位輸入並將資料寫入 Arduino EEPROM
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 驗證輸入格式（必須為 4 位元二進位）
    ''' 2. 轉換為十進位（0-15）
    ''' 3. 建立 EEPROM 寫入封包
    ''' 4. 傳送至 Arduino
    ''' 5. 顯示寫入成功訊息
    ''' 
    ''' 封包格式: [SOF][CMD][LEN][Data][CHK][EOF]
    ''' - SOF: 0xFF
    ''' - CMD: 'E' (0x45) = EEPROM Write
    ''' - LEN: 0x01（資料長度 = 1 byte）
    ''' - Data: 0x00 ~ 0x0F（十進位 0-15）
    ''' - CHK: Data & 0xFF
    ''' - EOF: 0xFE
    ''' 
    ''' 範例（二進位 1010 = 十進位 10）:
    ''' 0xFF 0x45 0x01 0x0A 0x0A 0xFE
    '''  ↑    ↑    ↑    ↑    ↑    ↑
    ''' SOF  CMD  LEN  值   CHK  EOF
    ''' </remarks>
    Private Sub ButtonWrite_Click(sender As Object, e As EventArgs) Handles ButtonWrite.Click
        ' 取得輸入框文字並去除前後空白
        Dim input As String = TextBoxBinary.Text.Trim()

        ' P8 驗證: 檢查是否為有效的 4 位元二進位格式
        ' ^[01]{4}$ 表示: 必須恰好 4 個字元，且每個字元只能是 0 或 1
        If Not Regex.IsMatch(input, "^[01]{4}$") Then
            ' 格式錯誤，顯示警告訊息
            MessageBox.Show("Not BIN Format", "格式錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            ' 清空輸入框
            TextBoxBinary.Clear()

            ' 重置十進位顯示為 0
            LabelDecimalValue.Text = "0"

            ' 結束程序
            Return
        End If

        Try
            ' 將 4 位元二進位轉換為十進位（0-15）
            Dim decimalValue As Integer = Convert.ToInt32(input, 2)

            ' 建立 EEPROM 寫入封包
            Dim packet As New List(Of Byte)

            ' [SOF] 封包起始標記
            packet.Add(&HFF)  ' 0xFF

            ' [CMD] 命令碼: EEPROM 寫入
            packet.Add(Asc("E"c))  ' 'E' = 0x45

            ' [LEN] 資料長度: 1 byte
            packet.Add(1)

            ' [Data] 要寫入的數值（0-15）
            packet.Add(CByte(decimalValue))

            ' [CHK] 計算檢查碼
            ' 因為只有一個資料位元組，所以 Checksum = 資料值本身
            Dim checksum As Byte = CByte(decimalValue And &HFF)
            packet.Add(checksum)

            ' [EOF] 封包結束標記
            packet.Add(&HFE)  ' 0xFE

            ' 檢查序列埠是否處於開啟狀態
            If SerialPort1.IsOpen Then
                ' 傳送封包至 Arduino
                SerialPort1.Write(packet.ToArray(), 0, packet.Count)

                ' 顯示寫入成功訊息
                ' 包含二進位和十進位的對應關係
                MessageBox.Show($"已寫入 EEPROM：二進位 {input} → 十進位 {decimalValue}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                ' 未連線，無法寫入
                MessageBox.Show("未連線，無法寫入 EEPROM", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            ' 捕捉寫入過程中的例外
            MessageBox.Show($"寫入失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================================================================
    ' P10: 程式結束
    '==========================================================================
    ''' <summary>
    ''' Exit 按鈕點擊事件
    ''' 關閉所有資源並結束程式
    ''' </summary>
    ''' <remarks>
    ''' 執行流程:
    ''' 1. 關閉序列埠連線（若處於開啟狀態）
    ''' 2. 釋放 CPU 效能計數器資源
    ''' 3. 釋放 RAM 效能計數器資源
    ''' 4. 停止並釋放 COM Port 監控器
    ''' 5. 呼叫 Application.Exit() 結束程式
    ''' </remarks>
    Private Sub ButtonExit_Click(sender As Object, e As EventArgs) Handles ButtonExit.Click
        ' 關閉序列埠連線
        If SerialPort1 IsNot Nothing AndAlso SerialPort1.IsOpen Then
            SerialPort1.Close()
        End If

        ' 釋放 CPU 效能計數器資源
        If cpuCounter IsNot Nothing Then
            cpuCounter.Dispose()
        End If

        ' 釋放 RAM 效能計數器資源
        If ramCounter IsNot Nothing Then
            ramCounter.Dispose()
        End If

        ' 停止並釋放 COM Port 監控器
        If portWatcher IsNot Nothing Then
            portWatcher.Stop()
            portWatcher.Dispose()
        End If

        ' 結束整個應用程式
        Application.Exit()
    End Sub

    '==========================================================================
    ' 輔助功能方法
    '==========================================================================
    ''' <summary>
    ''' 重置所有監控顯示數值為初始狀態
    ''' </summary>
    ''' <remarks>
    ''' 用於關閉連線或停止監控時
    ''' 將所有數值歸零並將顏色方塊設為灰色
    ''' </remarks>
    Private Sub ResetDisplayValues()
        ' 重置 CPU 使用率顯示為 0 %
        LabelCPUValue.Text = "0 %"

        ' 重置 RAM 使用率顯示為 0 %
        LabelRAMValue.Text = "0 %"

        ' 將 CPU 顏色方塊設為灰色（預設狀態）
        PanelCPUColor.BackColor = Color.Gray

        ' 將 RAM 顏色方塊設為灰色（預設狀態）
        PanelRAMColor.BackColor = Color.Gray
    End Sub

    '==========================================================================
    ' COM Port 熱插拔監控
    '==========================================================================
    ''' <summary>
    ''' 初始化 COM Port 熱插拔監控器
    ''' 使用 WMI 事件監聽 USB 裝置的插入與移除
    ''' </summary>
    ''' <remarks>
    ''' 監控原理:
    ''' - 使用 WMI (Windows Management Instrumentation) 查詢
    ''' - 監聽 Win32_DeviceChangeEvent 事件
    ''' - EventType = 2: 裝置插入
    ''' - EventType = 3: 裝置移除
    ''' 
    ''' 當偵測到裝置變化時，會自動觸發 OnPortChanged 事件
    ''' 然後重新載入 COM Port 清單
    ''' </remarks>
    Private Sub InitializePortWatcher()
        Try
            ' 建立 WMI 事件查詢
            ' 監聽裝置變更事件（插入或移除）
            Dim query As New WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3")

            ' 建立事件監視器
            portWatcher = New ManagementEventWatcher(query)

            ' 註冊事件處理程序
            ' 當偵測到裝置變化時，會呼叫 OnPortChanged 方法
            AddHandler portWatcher.EventArrived, AddressOf OnPortChanged

            ' 啟動監視器
            portWatcher.Start()

        Catch ex As Exception
            ' 初始化失敗時採用靜默處理
            ' 不影響主要功能，只是無法自動偵測 COM Port 變化
            ' 使用者仍可手動點擊「重新整理」按鈕
        End Try
    End Sub

    ''' <summary>
    ''' COM Port 變化事件處理程序
    ''' 當偵測到 USB 裝置插入或移除時觸發
    ''' </summary>
    ''' <remarks>
    ''' 因為 WMI 事件在背景執行緒中觸發
    ''' 需要使用 Invoke 將操作切換到 UI 執行緒
    ''' 否則會發生跨執行緒存取錯誤
    ''' </remarks>
    Private Sub OnPortChanged(sender As Object, e As EventArgs)
        ' 檢查是否需要跨執行緒呼叫
        ' InvokeRequired = True 表示目前不在 UI 執行緒中
        If Me.InvokeRequired Then
            ' 使用 Invoke 將方法呼叫轉送到 UI 執行緒
            ' 這樣才能安全地更新 UI 元件
            Me.Invoke(New Action(AddressOf LoadComPorts))
        Else
            ' 已經在 UI 執行緒中，直接呼叫
            LoadComPorts()
        End If
    End Sub

    '==========================================================================
    ' 表單關閉事件
    '==========================================================================
    ''' <summary>
    ''' 表單關閉事件處理程序
    ''' 確保所有資源都被正確釋放
    ''' </summary>
    ''' <remarks>
    ''' 此方法會在以下情況觸發:
    ''' - 使用者點擊視窗右上角的關閉按鈕 (X)
    ''' - 按下 Alt+F4
    ''' - 透過工作管理員結束程式
    ''' 
    ''' 執行流程與 Exit 按鈕相同，確保資源正確釋放
    ''' </remarks>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' 關閉序列埠連線
        If SerialPort1 IsNot Nothing AndAlso SerialPort1.IsOpen Then
            SerialPort1.Close()
        End If

        ' 釋放 CPU 效能計數器資源
        If cpuCounter IsNot Nothing Then
            cpuCounter.Dispose()
        End If

        ' 釋放 RAM 效能計數器資源
        If ramCounter IsNot Nothing Then
            ramCounter.Dispose()
        End If

        ' 停止並釋放 COM Port 監控器
        If portWatcher IsNot Nothing Then
            portWatcher.Stop()
            portWatcher.Dispose()
        End If

        ' 呼叫基底類別的 OnFormClosing 方法
        ' 完成預設的關閉流程
        MyBase.OnFormClosing(e)
    End Sub

End Class

'==============================================================================
' 程式碼結束
'==============================================================================
