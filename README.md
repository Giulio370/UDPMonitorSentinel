This application checks that the authorization file "C:\Certificate\PlayerCertificate.txt" is present. 
After that, it decrypts it with the User key (32 characters) and checks that the authorized monitor is connected to the computer. The key must be the same used in the "SecureMonitorAuthorize" program.
Providing a bool response on udp request


You need to enter your key and port in "Program.cs"
This program needs the "DisplayScanner.exe" app in the same directory (Rename it "AppVerificaMonitor_Ver2.exe")

The authorization file is created by the "SecureMonitorAuthorize" program. Rename it "PlayerCertificate.txt" and place it in "C:\Certificate\.."
The UDP request must have the message "checkMonitor" through the port you entered
