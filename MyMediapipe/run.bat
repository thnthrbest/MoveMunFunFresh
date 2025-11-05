@echo off
call "C:\Users\comsc\Documents\GitHub\MoveMunFunFresh\MyMediapipe\venv\Scripts\activate.bat"
python C:\Users\comsc\Documents\GitHub\MoveMunFunFresh\MyMediapipe\MediapipeBodyTracker.py
echo Close in 2 Seconds
TIMEOUT /T 2 /NOBREAK
exit