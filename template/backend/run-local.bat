@echo off
setlocal

echo ============================================================
echo   Ambev Developer Evaluation - Sales Order
echo   Subindo infra (Docker) + WebApi + OutboxProcessor
echo ============================================================
echo.

cd /d "%~dp0"

echo [1/3] Subindo Postgres, RabbitMQ, Redis e MongoDB via Docker Compose...
docker compose up -d --wait ambev.developerevaluation.database ambev.developerevaluation.rabbitmq ambev.developerevaluation.cache ambev.developerevaluation.nosql
if errorlevel 1 (
    echo.
    echo Falha ao subir os containers. Verifique se o Docker Desktop esta em execucao.
    pause
    exit /b 1
)

echo.
echo [2/3] Aplicando migrations pendentes no Postgres...
dotnet ef database update --project src\Ambev.DeveloperEvaluation.ORM --startup-project src\Ambev.DeveloperEvaluation.WebApi
if errorlevel 1 (
    echo.
    echo Falha ao aplicar as migrations. Confira a mensagem acima antes de continuar.
    pause
    exit /b 1
)

echo.
echo [3/3] Subindo WebApi e OutboxProcessor, cada um na sua janela...
start "Ambev WebApi" cmd /k "cd /d "%~dp0src\Ambev.DeveloperEvaluation.WebApi" && dotnet run"
start "Ambev OutboxProcessor" cmd /k "cd /d "%~dp0src\Ambev.DeveloperEvaluation.OutboxProcessor" && dotnet run"

echo.
echo ============================================================
echo  Tudo no ar.
echo  Swagger .......... https://localhost:7181/swagger
echo  RabbitMQ UI ...... http://localhost:15672 (guest / guest)
echo  Fechar tudo: feche as duas janelas abertas e rode
echo               "docker compose down" nesta pasta.
echo ============================================================
echo.
pause
