# Рабочее окружение
1. **Visual Studio Code, .NET Core 2.0**
  https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/with-visual-studio-code
2. Key-value хранилище **Redis**
  * Можно установить в обычном режиме из дистрибутива https://redis.io/download
  * Или в виде docker контейнера https://hub.docker.com/_/redis/
3. Брокер очередей **RabbitMQ**
  * Также можно установить как из дистрибутива https://www.rabbitmq.com/#getstarted
  * Так и в виде docker контейнера https://hub.docker.com/_/rabbitmq/


# Задание №1 Синхронное взаимодействие компонентов (HTTP REST)
Необходимо разработать приложение, состоящее из двух компонентов: **Frontend** и **Backend**, взаимодействующих по протоколу HTTP.

## Компонент **Frontend**
ASP.Net MVC приложение. Предоставляет форму ввода, кнопку отсылки формы на серверную часть. 
Серверная часть формирует отправляет введённые данные в компонент **Backend** посредством HTTP-запроса.

Создание проекта в VSCode:
1. Открыть папку *Frontend*.
2. В терминале выполнить **dotnet new mvc**
3. В файле *Controllers/HomeController.cs* добавить методы *Upload*
4. Реализовать TODO часть.
  Для выполнения HTTP запроса удобнее всего воспользоваться классом *HttpClient*
  https://docs.microsoft.com/ru-ru/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
5.	Создать файл *Views/Home/Upload.cshtml* с формой ввода
6.	Запустить компонент: в Integrated terminal выполнить **dotnet run**
  Форма ввода будет доступна в браузере по адресу http://localhost:5000/Home/Upload

## Компонент **Backend**
ASP.Net WebApi приложение. Сервис-хранилище вводимых данных.
При запросе на сохранение генерирует уникальный ключ и помещает в память данные по этому ключу. 
Входные параметрты: string value - данные для сохранения. 
Ответ: строка - значение ключа, по которому можно получить данные.
При запросе на получение компонент возвращает значение из памяти по ключу.
Входные параметры: string id – ключ
Ответ: строка – сохранённые по ключу данные.

Создание проекта в VSCode:
1.	Открыть папку *Backend*.
2.	В терминале выполнить **dotnet new webapi**
3.	В файле *Controllers/ValuesController.cs* добавить код обрботки запросов (в примере готово)
4.	Запустить компонент: в Integrated terminal выполнить **dotnet run**
