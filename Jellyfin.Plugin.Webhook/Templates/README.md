## Templates
### Ntfy
The configuration page includes an **Add Ntfy Destination** button that creates a pre-filled Generic destination.

Webhook URL:
```
https://ntfy.sh

#or

https://yourntfyurl.tld
```

Use the ntfy root URL, not a topic URL. The default preset sends JSON and uses a `NtfyTopic` field with the value `jellyfin`.

Required request headers:
```
Key:
Content-Type

Value:
application/json

Key:
X-Markdown

Value:
true
```

If your ntfy needs authorization headers then add a request header as follows:
```
Key:
Authorization

Value:
Basic <base64encodedpassword>
```
