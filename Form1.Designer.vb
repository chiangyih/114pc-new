<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        GroupBoxConnection = New GroupBox()
        ButtonRefresh = New Button()
        ButtonClose = New Button()
        ButtonOpen = New Button()
        LabelConnectionStatus = New Label()
        Label2 = New Label()
        ComboBoxCOM = New ComboBox()
        Label1 = New Label()
        GroupBoxCPU = New GroupBox()
        PanelCPUColor = New Panel()
        ButtonStopCPU = New Button()
        ButtonStartCPU = New Button()
        LabelCPUValue = New Label()
        Label3 = New Label()
        GroupBoxRAM = New GroupBox()
        PanelRAMColor = New Panel()
        ButtonStopRAM = New Button()
        ButtonStartRAM = New Button()
        LabelRAMValue = New Label()
        Label5 = New Label()
        GroupBoxEEPROM = New GroupBox()
        LabelDecimalValue = New Label()
        Label7 = New Label()
        ButtonWrite = New Button()
        TextBoxBinary = New TextBox()
        Label6 = New Label()
        GroupBoxBluetooth = New GroupBox()
        LabelBluetoothName = New Label()
        Label8 = New Label()
        ButtonExit = New Button()
        SerialPort1 = New System.IO.Ports.SerialPort(components)
        TimerCPU = New Timer(components)
        TimerRAM = New Timer(components)
        GroupBoxConnection.SuspendLayout()
        GroupBoxCPU.SuspendLayout()
        GroupBoxRAM.SuspendLayout()
        GroupBoxEEPROM.SuspendLayout()
        GroupBoxBluetooth.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBoxConnection
        ' 
        GroupBoxConnection.Controls.Add(ButtonRefresh)
        GroupBoxConnection.Controls.Add(ButtonClose)
        GroupBoxConnection.Controls.Add(ButtonOpen)
        GroupBoxConnection.Controls.Add(LabelConnectionStatus)
        GroupBoxConnection.Controls.Add(Label2)
        GroupBoxConnection.Controls.Add(ComboBoxCOM)
        GroupBoxConnection.Controls.Add(Label1)
        GroupBoxConnection.Location = New Point(12, 12)
        GroupBoxConnection.Name = "GroupBoxConnection"
        GroupBoxConnection.Size = New Size(450, 150)
        GroupBoxConnection.TabIndex = 0
        GroupBoxConnection.TabStop = False
        GroupBoxConnection.Text = "COM Port 連線管理"
        ' 
        ' ButtonRefresh
        ' 
        ButtonRefresh.Location = New Point(310, 35)
        ButtonRefresh.Name = "ButtonRefresh"
        ButtonRefresh.Size = New Size(120, 30)
        ButtonRefresh.TabIndex = 6
        ButtonRefresh.Text = "重新整理"
        ButtonRefresh.UseVisualStyleBackColor = True
        ' 
        ' ButtonClose
        ' 
        ButtonClose.Enabled = False
        ButtonClose.Location = New Point(160, 80)
        ButtonClose.Name = "ButtonClose"
        ButtonClose.Size = New Size(120, 40)
        ButtonClose.TabIndex = 5
        ButtonClose.Text = "Close"
        ButtonClose.UseVisualStyleBackColor = True
        ' 
        ' ButtonOpen
        ' 
        ButtonOpen.Location = New Point(20, 80)
        ButtonOpen.Name = "ButtonOpen"
        ButtonOpen.Size = New Size(120, 40)
        ButtonOpen.TabIndex = 4
        ButtonOpen.Text = "Open"
        ButtonOpen.UseVisualStyleBackColor = True
        ' 
        ' LabelConnectionStatus
        ' 
        LabelConnectionStatus.AutoSize = True
        LabelConnectionStatus.Font = New Font("Microsoft JhengHei UI", 12.0F, FontStyle.Bold)
        LabelConnectionStatus.ForeColor = Color.Red
        LabelConnectionStatus.Location = New Point(345, 92)
        LabelConnectionStatus.Name = "LabelConnectionStatus"
        LabelConnectionStatus.Size = New Size(94, 20)
        LabelConnectionStatus.TabIndex = 3
        LabelConnectionStatus.Text = "Disconnect"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(284, 95)
        Label2.Name = "Label2"
        Label2.Size = New Size(67, 15)
        Label2.TabIndex = 2
        Label2.Text = "連線狀態："
        ' 
        ' ComboBoxCOM
        ' 
        ComboBoxCOM.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBoxCOM.FormattingEnabled = True
        ComboBoxCOM.Location = New Point(110, 38)
        ComboBoxCOM.Name = "ComboBoxCOM"
        ComboBoxCOM.Size = New Size(180, 23)
        ComboBoxCOM.TabIndex = 1
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(20, 41)
        Label1.Name = "Label1"
        Label1.Size = New Size(75, 15)
        Label1.TabIndex = 0
        Label1.Text = "COM Port："
        ' 
        ' GroupBoxCPU
        ' 
        GroupBoxCPU.Controls.Add(PanelCPUColor)
        GroupBoxCPU.Controls.Add(ButtonStopCPU)
        GroupBoxCPU.Controls.Add(ButtonStartCPU)
        GroupBoxCPU.Controls.Add(LabelCPUValue)
        GroupBoxCPU.Controls.Add(Label3)
        GroupBoxCPU.Location = New Point(12, 180)
        GroupBoxCPU.Name = "GroupBoxCPU"
        GroupBoxCPU.Size = New Size(450, 120)
        GroupBoxCPU.TabIndex = 1
        GroupBoxCPU.TabStop = False
        GroupBoxCPU.Text = "CPU Loading 監控"
        ' 
        ' PanelCPUColor
        ' 
        PanelCPUColor.BackColor = Color.Gray
        PanelCPUColor.BorderStyle = BorderStyle.FixedSingle
        PanelCPUColor.Location = New Point(310, 30)
        PanelCPUColor.Name = "PanelCPUColor"
        PanelCPUColor.Size = New Size(120, 70)
        PanelCPUColor.TabIndex = 4
        ' 
        ' ButtonStopCPU
        ' 
        ButtonStopCPU.Enabled = False
        ButtonStopCPU.Location = New Point(160, 65)
        ButtonStopCPU.Name = "ButtonStopCPU"
        ButtonStopCPU.Size = New Size(120, 35)
        ButtonStopCPU.TabIndex = 3
        ButtonStopCPU.Text = "Stop CPU"
        ButtonStopCPU.UseVisualStyleBackColor = True
        ' 
        ' ButtonStartCPU
        ' 
        ButtonStartCPU.Enabled = False
        ButtonStartCPU.Location = New Point(20, 65)
        ButtonStartCPU.Name = "ButtonStartCPU"
        ButtonStartCPU.Size = New Size(120, 35)
        ButtonStartCPU.TabIndex = 2
        ButtonStartCPU.Text = "Start CPU"
        ButtonStartCPU.UseVisualStyleBackColor = True
        ' 
        ' LabelCPUValue
        ' 
        LabelCPUValue.AutoSize = True
        LabelCPUValue.Font = New Font("Microsoft JhengHei UI", 14.0F, FontStyle.Bold)
        LabelCPUValue.Location = New Point(160, 30)
        LabelCPUValue.Name = "LabelCPUValue"
        LabelCPUValue.Size = New Size(43, 24)
        LabelCPUValue.TabIndex = 1
        LabelCPUValue.Text = "0 %"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(20, 35)
        Label3.Name = "Label3"
        Label3.Size = New Size(117, 15)
        Label3.TabIndex = 0
        Label3.Text = "CPU 使用率（%）："
        ' 
        ' GroupBoxRAM
        ' 
        GroupBoxRAM.Controls.Add(PanelRAMColor)
        GroupBoxRAM.Controls.Add(ButtonStopRAM)
        GroupBoxRAM.Controls.Add(ButtonStartRAM)
        GroupBoxRAM.Controls.Add(LabelRAMValue)
        GroupBoxRAM.Controls.Add(Label5)
        GroupBoxRAM.Location = New Point(12, 315)
        GroupBoxRAM.Name = "GroupBoxRAM"
        GroupBoxRAM.Size = New Size(450, 120)
        GroupBoxRAM.TabIndex = 2
        GroupBoxRAM.TabStop = False
        GroupBoxRAM.Text = "RAM Loading 監控"
        ' 
        ' PanelRAMColor
        ' 
        PanelRAMColor.BackColor = Color.Gray
        PanelRAMColor.BorderStyle = BorderStyle.FixedSingle
        PanelRAMColor.Location = New Point(310, 30)
        PanelRAMColor.Name = "PanelRAMColor"
        PanelRAMColor.Size = New Size(120, 70)
        PanelRAMColor.TabIndex = 5
        ' 
        ' ButtonStopRAM
        ' 
        ButtonStopRAM.Enabled = False
        ButtonStopRAM.Location = New Point(160, 65)
        ButtonStopRAM.Name = "ButtonStopRAM"
        ButtonStopRAM.Size = New Size(120, 35)
        ButtonStopRAM.TabIndex = 3
        ButtonStopRAM.Text = "Stop RAM"
        ButtonStopRAM.UseVisualStyleBackColor = True
        ' 
        ' ButtonStartRAM
        ' 
        ButtonStartRAM.Enabled = False
        ButtonStartRAM.Location = New Point(20, 65)
        ButtonStartRAM.Name = "ButtonStartRAM"
        ButtonStartRAM.Size = New Size(120, 35)
        ButtonStartRAM.TabIndex = 2
        ButtonStartRAM.Text = "Start RAM"
        ButtonStartRAM.UseVisualStyleBackColor = True
        ' 
        ' LabelRAMValue
        ' 
        LabelRAMValue.AutoSize = True
        LabelRAMValue.Font = New Font("Microsoft JhengHei UI", 14.0F, FontStyle.Bold)
        LabelRAMValue.Location = New Point(160, 30)
        LabelRAMValue.Name = "LabelRAMValue"
        LabelRAMValue.Size = New Size(43, 24)
        LabelRAMValue.TabIndex = 1
        LabelRAMValue.Text = "0 %"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(20, 35)
        Label5.Name = "Label5"
        Label5.Size = New Size(121, 15)
        Label5.TabIndex = 0
        Label5.Text = "RAM 使用率（%）："
        ' 
        ' GroupBoxEEPROM
        ' 
        GroupBoxEEPROM.Controls.Add(LabelDecimalValue)
        GroupBoxEEPROM.Controls.Add(Label7)
        GroupBoxEEPROM.Controls.Add(ButtonWrite)
        GroupBoxEEPROM.Controls.Add(TextBoxBinary)
        GroupBoxEEPROM.Controls.Add(Label6)
        GroupBoxEEPROM.Location = New Point(480, 12)
        GroupBoxEEPROM.Name = "GroupBoxEEPROM"
        GroupBoxEEPROM.Size = New Size(300, 180)
        GroupBoxEEPROM.TabIndex = 3
        GroupBoxEEPROM.TabStop = False
        GroupBoxEEPROM.Text = "EEPROM 資料寫入"
        ' 
        ' LabelDecimalValue
        ' 
        LabelDecimalValue.AutoSize = True
        LabelDecimalValue.Font = New Font("Microsoft JhengHei UI", 12.0F, FontStyle.Bold)
        LabelDecimalValue.ForeColor = Color.Blue
        LabelDecimalValue.Location = New Point(140, 85)
        LabelDecimalValue.Name = "LabelDecimalValue"
        LabelDecimalValue.Size = New Size(19, 20)
        LabelDecimalValue.TabIndex = 4
        LabelDecimalValue.Text = "0"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(20, 88)
        Label7.Name = "Label7"
        Label7.Size = New Size(91, 15)
        Label7.TabIndex = 3
        Label7.Text = "十進位轉換值："
        ' 
        ' ButtonWrite
        ' 
        ButtonWrite.Enabled = False
        ButtonWrite.Font = New Font("Microsoft JhengHei UI", 12.0F, FontStyle.Bold)
        ButtonWrite.Location = New Point(70, 125)
        ButtonWrite.Name = "ButtonWrite"
        ButtonWrite.Size = New Size(160, 40)
        ButtonWrite.TabIndex = 2
        ButtonWrite.Text = "Write to EEPROM"
        ButtonWrite.UseVisualStyleBackColor = True
        ' 
        ' TextBoxBinary
        ' 
        TextBoxBinary.Font = New Font("Consolas", 14.0F)
        TextBoxBinary.Location = New Point(140, 40)
        TextBoxBinary.MaxLength = 4
        TextBoxBinary.Name = "TextBoxBinary"
        TextBoxBinary.Size = New Size(120, 29)
        TextBoxBinary.TabIndex = 1
        TextBoxBinary.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(20, 47)
        Label6.Name = "Label6"
        Label6.Size = New Size(91, 15)
        Label6.TabIndex = 0
        Label6.Text = "輸入二進位值："
        ' 
        ' GroupBoxBluetooth
        ' 
        GroupBoxBluetooth.Controls.Add(LabelBluetoothName)
        GroupBoxBluetooth.Controls.Add(Label8)
        GroupBoxBluetooth.Location = New Point(480, 210)
        GroupBoxBluetooth.Name = "GroupBoxBluetooth"
        GroupBoxBluetooth.Size = New Size(300, 100)
        GroupBoxBluetooth.TabIndex = 4
        GroupBoxBluetooth.TabStop = False
        GroupBoxBluetooth.Text = "藍牙模組資訊"
        ' 
        ' LabelBluetoothName
        ' 
        LabelBluetoothName.AutoSize = True
        LabelBluetoothName.Font = New Font("Consolas", 12.0F, FontStyle.Bold)
        LabelBluetoothName.ForeColor = Color.DarkBlue
        LabelBluetoothName.Location = New Point(20, 55)
        LabelBluetoothName.Name = "LabelBluetoothName"
        LabelBluetoothName.Size = New Size(108, 19)
        LabelBluetoothName.TabIndex = 1
        LabelBluetoothName.Text = "ODD-01-0001"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(20, 30)
        Label8.Name = "Label8"
        Label8.Size = New Size(91, 15)
        Label8.TabIndex = 0
        Label8.Text = "模組名稱命名："
        ' 
        ' ButtonExit
        ' 
        ButtonExit.BackColor = Color.FromArgb(CByte(255), CByte(192), CByte(192))
        ButtonExit.Font = New Font("Microsoft JhengHei UI", 12.0F, FontStyle.Bold)
        ButtonExit.Location = New Point(580, 360)
        ButtonExit.Name = "ButtonExit"
        ButtonExit.Size = New Size(160, 50)
        ButtonExit.TabIndex = 5
        ButtonExit.Text = "Exit"
        ButtonExit.UseVisualStyleBackColor = False
        ' 
        ' SerialPort1
        ' 
        SerialPort1.BaudRate = 9600
        SerialPort1.DataBits = 8
        SerialPort1.DiscardNull = False
        SerialPort1.DtrEnable = False
        SerialPort1.Encoding = System.Text.Encoding.ASCII
        SerialPort1.Handshake = IO.Ports.Handshake.None
        SerialPort1.NewLine = vbLf
        SerialPort1.Parity = IO.Ports.Parity.None
        SerialPort1.ParityReplace = CByte(63)
        SerialPort1.PortName = "COM1"
        SerialPort1.ReadBufferSize = 4096
        SerialPort1.ReadTimeout = -1
        SerialPort1.ReceivedBytesThreshold = 1
        SerialPort1.RtsEnable = False
        SerialPort1.StopBits = IO.Ports.StopBits.One
        SerialPort1.WriteBufferSize = 2048
        SerialPort1.WriteTimeout = -1
        ' 
        ' TimerCPU
        ' 
        TimerCPU.Interval = 1000
        ' 
        ' TimerRAM
        ' 
        TimerRAM.Interval = 1000
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(ButtonExit)
        Controls.Add(GroupBoxBluetooth)
        Controls.Add(GroupBoxEEPROM)
        Controls.Add(GroupBoxRAM)
        Controls.Add(GroupBoxCPU)
        Controls.Add(GroupBoxConnection)
        FormBorderStyle = FormBorderStyle.FixedSingle
        MaximizeBox = False
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Form1"
        GroupBoxConnection.ResumeLayout(False)
        GroupBoxConnection.PerformLayout()
        GroupBoxCPU.ResumeLayout(False)
        GroupBoxCPU.PerformLayout()
        GroupBoxRAM.ResumeLayout(False)
        GroupBoxRAM.PerformLayout()
        GroupBoxEEPROM.ResumeLayout(False)
        GroupBoxEEPROM.PerformLayout()
        GroupBoxBluetooth.ResumeLayout(False)
        GroupBoxBluetooth.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents GroupBoxConnection As GroupBox
    Friend WithEvents ButtonRefresh As Button
    Friend WithEvents ButtonClose As Button
    Friend WithEvents ButtonOpen As Button
    Friend WithEvents LabelConnectionStatus As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents ComboBoxCOM As ComboBox
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBoxCPU As GroupBox
    Friend WithEvents PanelCPUColor As Panel
    Friend WithEvents ButtonStopCPU As Button
    Friend WithEvents ButtonStartCPU As Button
    Friend WithEvents LabelCPUValue As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents GroupBoxRAM As GroupBox
    Friend WithEvents PanelRAMColor As Panel
    Friend WithEvents ButtonStopRAM As Button
    Friend WithEvents ButtonStartRAM As Button
    Friend WithEvents LabelRAMValue As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents GroupBoxEEPROM As GroupBox
    Friend WithEvents LabelDecimalValue As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents ButtonWrite As Button
    Friend WithEvents TextBoxBinary As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents GroupBoxBluetooth As GroupBox
    Friend WithEvents LabelBluetoothName As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents ButtonExit As Button
    Friend WithEvents SerialPort1 As System.IO.Ports.SerialPort
    Friend WithEvents TimerCPU As Timer
    Friend WithEvents TimerRAM As Timer

End Class
