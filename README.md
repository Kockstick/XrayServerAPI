Данный проект предназначен для быстроко разворачивания ВПН-сервера Xray и управления доступом.

# Установка

Установить .Net 9.0
```
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-9.0
dotnet --version
```

Клонировать репозиторий в папку home
```
git clone https://github.com/Kockstick/XrayServerAPI.git
```

Зайти в папку XrayServerAPI
```
cd ./XrayServerAPI
```

Собрать проект
```
dotnet publish -c Release -o out
```

Задать домен
```
grep -qxF 'export DOMAIN={домен сервера}' ~/.bashrc || echo 'export DOMAIN={домен сервера}' >> ~/.bashrc
source ~/.bashrc
```

# Запуск

Запуск в фоне с возможностью вернуться к процессу:
```
screen -S xray  
dotnet out/XrayServerAPI.dll
```
Чтобы отсоединиться: **Ctrl + A, потом D**

Вернуться:
```
screen -r xray
```

*Сервер выдаст в логах вот такую строку: "API: https://ДОМЕН/rVSKwQ3JQH2ttB8IQUbwegwx8udwep39NPl/01tE/jqfdc=", где rVSKwQ3JQH2ttB8IQUbwegwx8udwep39NPl/01tE/jqfdc= - это АПИ ключ. 
Его необходимо приложить в заголовке запроса "X-API-KEY"*

# API

*Для взаимодействия обращаться по адресу https://ДОМЕН/key/access-keys*

### HttpPost запрос создает ключ
Возвращает созданный ключ
```
public class XrayKey
{
    public string Id {  get; set; } //UUID
    public string Host { get; set; }
    public int Port { get; set; }
    public string AccessKey { get; set; }
}
```

### HttpDelete удаляет ключ
Возвращает удаленный ключ
```
XrayKey?
```

### HttpGet возвразает список ключей
Возвращает список ключей
```
List<XrayKey>? keys
```

### HttpPut сообщает существует ли такой ключ
Возвращает true или false
