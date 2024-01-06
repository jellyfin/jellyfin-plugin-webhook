# Pushover Templates

## Supported notification types

- Authentication Failure
- Authentication Success
- Generic
- Item added
- Pending restart
- Playback progress
- Playback start
- Playback stop
- Plugin installation cancelled
- Plugin installation failed
- Plugin installed
- Plugin installing
- Plugin uninstalled
- Plugin updated
- Session started
- Task completed
- User created
- User deleted
- User locked out
- User password changed

## Usage

1. Click on the Webhook plugin
2. Click on ``Add Generic``
3. Enter a name
4. Enter https://api.pushover.net/1/messages.json as a Webhook URL
5. Check your desired notification types
6. Paste the desired template (recommended is [All in One](./AllInOne.handlebars))
7. Add request header (Key: ``Content-Type``, Value: ``application/json``)
8. Add field (Key: ``Token``, Value: **Your Pushover application token**)
9. Add field (Key: ``UserToken``, Value: **Your Pushover user key**)
10. Click ``Save``

## Debug
To debug, you can use https://webhook.site. Ensure that the server can resolve the hostname.

1. Go to https://webhook.site. **Never leave this page during debugging**.
2. Copy the webhook URL, ending with a random UID
3. Replace the ``Webhook URL`` to this URL
4. Press ``Save``
5. (Optional): Set up XHR Redirec to Pushover (can help with validation problems):
   - Go to your webhook debugging page again.
   - Click ``Settings...`` besides ``XHR Redirect`` in the top navigation bar
   - Enter ``https://api.pushover.net/1/messages.json`` as the target
   - Enter ``application/json`` as the content type
   - Select ``POST`` as the ``HTTP Method``
   - Press ``Close``
   - Ensure that ``XHR Redirect`` in the navbar is checked
   - Press ``CTRL`` + ``SHIFT`` + ``I``, click on ``Network`` and enter ``api.pushover`` in the filter field.
   - Any failed / successful notifications will appear here now. Your client will be used as the XHR redirect.
