## HMAC-Web-API

Client and server code for using HMAC to authenticate Web API Communication. The code will also protect against replay attacks.

This is implemented as two separate DLLs, one for the server which contains WebAPI attributes and one for the client which wraps the HTTPClient class

Multiple public/private keys are supported on the server so you can assign different keys to each client.

