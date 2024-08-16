#Requires AutoHotkey v2.0

MacropadGui := Gui()
MacropadGui.Title := "Macropad"

; General stuff
F13::
{
	Run "OUTLOOK.EXE"
	Run "ONENOTE.EXE"
	Run "ms-teams.exe"
	Run "C:\Users\Morten Lang\AppData\Local\slack\slack.exe"
	Run "chrome.exe"
	Run "msedge.exe"
}
F14::Run "notepad++.exe"
F15::Run "C:\Users\Morten Lang\AppData\Local\TIDAL\TIDAL.exe"

; Work setup
; F16::
; F17::

; General stuff
F18::
{
	Run "calc.exe"
}

; Home setup
F19::
{
	WinMove 50, 180, 1800, 1200, "ahk_exe chrome.exe"
	WinMove 120, 100, 1800, 1200, "ahk_exe msedge.exe"
	
	WinMove 1700, 50, 1700, 1200, "ahk_exe outlook.exe"
	WinMove 2000, 450, 1300, 900, "ahk_exe onenote.exe"

	; Moving a window and resizing needs to be done in 2 steps, otherwise the size get's messed up
	WinMove -2310, 10,,, "ahk_exe slack.exe"
	WinMove ,, 1450, 960, "ahk_exe slack.exe"
	
	WinMove -2550, 120,,, "ahk_exe ms-teams.exe"
	WinMove ,, 1450, 900, "ahk_exe ms-teams.exe"
}
; F20::

; Misc
F21::
{
	Global MacropadGui
	
	if WinExist(MacropadGui.Hwnd) && WinActive(MacropadGui.Hwnd)
	{
		MacropadGui.Hide()
	}
	else
	{
		MacropadGui.AddText("x10 y10", "Open All Apps")
		MacropadGui.AddText("x110 y10", "Open Notepad++")
		MacropadGui.AddText("x210 y10", "Open Tidal")
		
		MacropadGui.AddText("x10 y30", "Work Positions")
		MacropadGui.AddText("x110 y30", "Daily Work Pos")
		MacropadGui.AddText("x210 y30", "Calculator")
		
		MacropadGui.AddText("x10 y50", "Home Positions")
		MacropadGui.AddText("x110 y50", "Daily Home Pos")
		MacropadGui.AddText("x210 y50", "Show Gui")
	
		MacropadGui.Show("x50 y50")
	}
}