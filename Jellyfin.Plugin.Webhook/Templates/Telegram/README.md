### Telegram
Webhook URL:
```
https://api.telegram.org/bot<id>:<token>/sendPhoto
```
To get the chatid:
 - start a chat with your bot
 - add `@get_id_bot`
 - issue the `/my_id` command
After adding the template add a request header as follows:
```
Key:
Content-Type

Value:
application/json
```