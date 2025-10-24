@echo off
call "C:\MyMediapipe\env\Scripts\activate.bat"
python C:\MyMediapipe\MediapipeBodyTracker.py
echo Close in 2 Seconds
TIMEOUT /T 2 /NOBREAK
exit