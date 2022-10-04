@echo off

if not defined PGSQL set PGSQL=%~dp0\pgsql\
if not defined PGDATABASE set PGDATABASE=monstertradingcardsgame
if not defined PGPORT set PGPORT=5432
if not defined PGUSER set PGUSER=postgres

echo.
echo Creating a copy...

"%PGSQL%\bin\pg_dump" --port=%PGPORT% --dbname="%PGDATABASE%" --username="%PGUSER%" > db_copy.sql

echo Copy successful!