cd C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\
gacutil /u Timer_Shell
gacutil /i "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Timer_Shell\Timer_Shell\bin\Debug\Timer_Shell.dll"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319\
RegAsm "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Timer_Shell\Timer_Shell\bin\Debug\Timer_Shell.dll"

cd C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\
gacutil /u Model
gacutil /i "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Model\Model\bin\Debug\Model.dll"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319\
RegAsm "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Model\Model\bin\Debug\Model.dll"


cd C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\
gacutil /i "C:\Users\AI_13_06\Desktop\Prototype\DLLS\TemporalReasonerX\TemporalReasonerX\bin\Debug\TemporalReasonerX.dll"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319\
RegAsm "C:\Users\AI_13_06\Desktop\Prototype\DLLS\TemporalReasonerX\TemporalReasonerX\bin\Debug\TemporalReasonerX.dll"

cd C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\
gacutil /i "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Interop.board.dll"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319\
RegAsm "C:\Users\AI_13_06\Desktop\Prototype\DLLS\Interop.board.dll"
