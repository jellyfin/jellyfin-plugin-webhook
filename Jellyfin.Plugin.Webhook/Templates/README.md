## Templates
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

### Ntfy
Webhook URL:
```
https://ntfy.sh

#or

https://yourntfyurl.tld
```

If your ntfy needs authorization headers then add a request header as follows:
```
Key:
Authorization

Value:
Basic <base64encodedpassword>
```
