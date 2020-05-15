@echo off

:: Copyright (c) Philipp Wagner. All rights reserved.
:: Licensed under the MIT license. See LICENSE file in the project root for full license information.

cd /D "%~dp0/../Backend"

docker build -t elasticsearch_fulltext -f ../Docker/Dockerfile .

echo\
echo -------------------------------
echo -- Docker Build has finished --
echo -------------------------------
echo\

call :AskQuestionWithYdefault "Do you want to run the Docker Container 'elasticsearch_fulltext' now [Y,n]?" reply_

if /i [%reply_%] EQU [y]  (
    docker run --rm -it \
        -v G:/Data:/Data 
        -p 9000:80 \ 
        -e ASPNETCORE_ENVIRONMENT=Linux \
        elasticsearch_fulltext
)

goto :end

:: The question as a subroutine
:AskQuestionWithYdefault
	setlocal enableextensions
	:_asktheyquestionagain
	set return_=
	set ask_=
	set /p ask_="%~1"
	if "%ask_%"=="" set return_=y
	if /i "%ask_%"=="Y" set return_=y
	if /i "%ask_%"=="n" set return_=n
	if not defined return_ goto _asktheyquestionagain
	endlocal & set "%2=%return_%" & goto :EOF

:end
pause